﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Billing.VISASecurityServices {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://10.156.7.25/visa/services/VISASecurityServices", ConfigurationName="VISASecurityServices.SecurityService")]
    public interface SecurityService {
        
        // CODEGEN: Generating message contract since the wrapper namespace (http://isoap.cpsoap.hyperlogy) of message checkServiceRequest does not match the default value (http://10.156.7.25/visa/services/VISASecurityServices)
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults=true, Use=System.ServiceModel.OperationFormatUse.Encoded)]
        Billing.VISASecurityServices.checkServiceResponse checkService(Billing.VISASecurityServices.checkServiceRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="", ReplyAction="*")]
        System.Threading.Tasks.Task<Billing.VISASecurityServices.checkServiceResponse> checkServiceAsync(Billing.VISASecurityServices.checkServiceRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="checkService", WrapperNamespace="http://isoap.cpsoap.hyperlogy", IsWrapped=true)]
    public partial class checkServiceRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="", Order=0)]
        public string mUsername;
        
        public checkServiceRequest() {
        }
        
        public checkServiceRequest(string mUsername) {
            this.mUsername = mUsername;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="checkServiceResponse", WrapperNamespace="http://10.156.7.25/visa/services/VISASecurityServices", IsWrapped=true)]
    public partial class checkServiceResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="", Order=0)]
        public string checkServiceReturn;
        
        public checkServiceResponse() {
        }
        
        public checkServiceResponse(string checkServiceReturn) {
            this.checkServiceReturn = checkServiceReturn;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface SecurityServiceChannel : Billing.VISASecurityServices.SecurityService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SecurityServiceClient : System.ServiceModel.ClientBase<Billing.VISASecurityServices.SecurityService>, Billing.VISASecurityServices.SecurityService {
        
        public SecurityServiceClient() {
        }
        
        public SecurityServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public SecurityServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SecurityServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SecurityServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Billing.VISASecurityServices.checkServiceResponse Billing.VISASecurityServices.SecurityService.checkService(Billing.VISASecurityServices.checkServiceRequest request) {
            return base.Channel.checkService(request);
        }
        
        public string checkService(string mUsername) {
            Billing.VISASecurityServices.checkServiceRequest inValue = new Billing.VISASecurityServices.checkServiceRequest();
            inValue.mUsername = mUsername;
            Billing.VISASecurityServices.checkServiceResponse retVal = ((Billing.VISASecurityServices.SecurityService)(this)).checkService(inValue);
            return retVal.checkServiceReturn;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<Billing.VISASecurityServices.checkServiceResponse> Billing.VISASecurityServices.SecurityService.checkServiceAsync(Billing.VISASecurityServices.checkServiceRequest request) {
            return base.Channel.checkServiceAsync(request);
        }
        
        public System.Threading.Tasks.Task<Billing.VISASecurityServices.checkServiceResponse> checkServiceAsync(string mUsername) {
            Billing.VISASecurityServices.checkServiceRequest inValue = new Billing.VISASecurityServices.checkServiceRequest();
            inValue.mUsername = mUsername;
            return ((Billing.VISASecurityServices.SecurityService)(this)).checkServiceAsync(inValue);
        }
    }
}