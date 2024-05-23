﻿using System;
using System.Collections.Generic;
using VRT.Orchestrator.Wrapping;

namespace VRT.Orchestrator.Responses { 
    public interface IOrchestratorResponseBody { }

    public class OrchestratorResponse<T>
    {
        public int error { get; set; }
        public string message { get; set; }

        public T body;

        public ResponseStatus ResponseStatus {
            get {
                return new ResponseStatus(error, message);
            }
        }
    }

    public class EmptyResponse : IOrchestratorResponseBody {}

    public class VersionResponse : IOrchestratorResponseBody {
        public string orchestratorVersion;
    }

    public class LoginResponse : IOrchestratorResponseBody {
        public string userId;
    }

    public class SessionUpdateEventData {
        public string userId;
        public User userData;
    }

    public class SessionUpdate {
        public string eventId;
        public SessionUpdateEventData eventData;
    }
}
