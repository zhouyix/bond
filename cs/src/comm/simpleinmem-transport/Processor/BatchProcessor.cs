// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Bond.Comm.SimpleInMem.Processor
{
    using System.Threading.Tasks;
    using Bond.Comm.Layers;
    using Bond.Comm.Service;

    internal class BatchProcessor : QueueProcessor
    {
        private const int MAXIMUM_BATCH_SIZE = 1000;
        
        internal BatchProcessor(SimpleInMemConnection connection, ServiceHost serviceHost) 
            : base(connection, serviceHost)
        {
        }

        /// <summary>
        /// A batch of <see cref="InMemFrame"/> instances are processed in each execution. They are dequeued from
        /// <see cref="SimpleInMemConnection.ReadQueue"/>. Single batch size is minimum of
        /// <see cref="MAXIMUM_BATCH_SIZE"/> and size of <see cref="SimpleInMemConnection.ReadQueue"/>.
        /// </summary>
        override internal void Process()
        {
            int batchIndex = 0;
            InMemFrameQueue readQueue = connection.ReadQueue;
            InMemFrameQueue writeQueue = connection.WriteQueue;

            if (readQueue == null || writeQueue == null)
            {
                return;
            }

            while (batchIndex < MAXIMUM_BATCH_SIZE && readQueue.Count > 0)
            {
                var payload = readQueue.Dequeue();

                if (payload == null)
                {
                    break;
                }

                switch(payload.headers.payload_type)
                {
                    case PayloadType.Event:
                        Task.Run(() => DispatchEvent(payload));
                        break;

                    case PayloadType.Request:
                        Task.Run(() => DispatchRequest(payload, writeQueue));
                        break;

                    case PayloadType.Response:
                        Task.Run(() => DispatchResponse(payload));
                        break;

                    default:
                        Log.Site().Error("Unsupported Payload type: [{0}], for conversation id: {1}.",
                                         payload.headers.payload_type, payload.headers.conversation_id);
                        break;
                }

                batchIndex++;
            }

        }

        private async void DispatchRequest(InMemFrame payload,
                                           InMemFrameQueue queue)
        {
            var receiveContext = new SimpleInMemReceiveContext(connection);
            var headers = payload.headers;
            var layerData = payload.layerData;
            var message = payload.message;
            var taskSource = payload.outstandingRequest;

            Error layerError = LayerStackUtils.ProcessOnReceive(this.serviceHost.ParentTransport.LayerStack,
                                                                MessageType.Request, receiveContext, layerData);

            IMessage response;

            if (layerError == null)
            {
                response = await serviceHost.DispatchRequest(headers.method_name, receiveContext, message, connection.ConnectionMetrics);
            }
            else
            {
                Log.Site().Error("Receiving request {0}/{1} failed due to layer error (Code: {2}, Message: {3}).",
                                 headers.conversation_id, headers.method_name, layerError.error_code, layerError.message);
                response = Message.FromError(layerError);
            }
            SendReply(headers.conversation_id, response, taskSource, queue);
        }

        internal void SendReply(ulong conversationId, 
                                IMessage response, 
                                TaskCompletionSource<IMessage> taskSource, 
                                InMemFrameQueue queue)
        {
            var sendContext = new SimpleInMemSendContext(connection);
            IBonded layerData;
            var layerError = LayerStackUtils.ProcessOnSend(this.serviceHost.ParentTransport.LayerStack,
                                                             MessageType.Response, sendContext, out layerData);

            // If there was a layer error, replace the response with the layer error
            if (layerError != null)
            {
                Log.Site().Error("Sending reply for conversation {0} failed due to layer error (Code: {1}, Message: {2}).",
                                 conversationId, layerError.error_code, layerError.message);
                response = Message.FromError(layerError);
            }

            var payload = Util.NewPayLoad(conversationId, PayloadType.Response, layerData, response, taskSource);
            queue.Enqueue(payload);
        }

        private void DispatchResponse(InMemFrame payload)
        {
            var receiveContext = new SimpleInMemReceiveContext(connection);
            var headers = payload.headers;
            var layerData = payload.layerData;
            var message = payload.message;
            var taskSource = payload.outstandingRequest;
            var layerError = LayerStackUtils.ProcessOnReceive(serviceHost.ParentTransport.LayerStack,
                                                                MessageType.Response, receiveContext, layerData);

            if (layerError != null)
            {
                Log.Site().Error("Receiving response {0}/{1} failed due to layer error (Code: {2}, Message: {3}).",
                                 headers.conversation_id, headers.method_name, layerError.error_code, layerError.message);
                message = Message.FromError(layerError);
            }

            payload.outstandingRequest.SetResult(message);
        }

        private async void DispatchEvent(InMemFrame payload)
        {
            var receiveContext = new SimpleInMemReceiveContext(connection);
            var headers = payload.headers;
            var layerData = payload.layerData;
            var message = payload.message;
            var taskSource = payload.outstandingRequest;

            Error layerError = LayerStackUtils.ProcessOnReceive(serviceHost.ParentTransport.LayerStack,
                                                                MessageType.Event, receiveContext, layerData);

            if (layerError != null)
            {
                Log.Site().Error("Receiving event {0}/{1} failed due to layer error (Code: {2}, Message: {3}).",
                                 headers.conversation_id, headers.method_name, layerError.error_code, layerError.message);
                return;
            }

            await serviceHost.DispatchEvent(
                    headers.method_name, receiveContext, message, connection.ConnectionMetrics);
        }
    }
}