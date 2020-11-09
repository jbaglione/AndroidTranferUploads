﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AndroidTranferUploads.Core.WSWebApps {
    using System.Data;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://tempuri.org", ConfigurationName="WSWebApps.WebAppsSoap")]
    public interface WebAppsSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.ChangePassword", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet ChangePassword(long pUsrExtId, string pOld, string pNew);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.ChangePassword", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> ChangePasswordAsync(long pUsrExtId, string pOld, string pNew);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.ForgotPassword", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet ForgotPassword(string pIde);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.ForgotPassword", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> ForgotPasswordAsync(string pIde);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.GetAlertas", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GetAlertas(long pUsrExtId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.GetAlertas", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> GetAlertasAsync(long pUsrExtId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.GetSenders", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GetSenders(long pApl);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.GetSenders", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> GetSendersAsync(long pApl);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.GetSessionData", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet GetSessionData(string pIde);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.GetSessionData", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> GetSessionDataAsync(string pIde);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.Login", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet Login(string pIde, string pPsw);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.Login", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> LoginAsync(string pIde, string pPsw);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.LoginMobileGerencial", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet LoginMobileGerencial(string pIde, string pPsw);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.LoginMobileGerencial", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> LoginMobileGerencialAsync(string pIde, string pPsw);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.SetPersonals", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataSet SetPersonals(long pUsrExtId, string pEmail);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/WebServices.WebApps.SetPersonals", ReplyAction="*")]
        System.Threading.Tasks.Task<System.Data.DataSet> SetPersonalsAsync(long pUsrExtId, string pEmail);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface WebAppsSoapChannel : AndroidTranferUploads.Core.WSWebApps.WebAppsSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WebAppsSoapClient : System.ServiceModel.ClientBase<AndroidTranferUploads.Core.WSWebApps.WebAppsSoap>, AndroidTranferUploads.Core.WSWebApps.WebAppsSoap {
        
        public WebAppsSoapClient() {
        }
        
        public WebAppsSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WebAppsSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WebAppsSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WebAppsSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public System.Data.DataSet ChangePassword(long pUsrExtId, string pOld, string pNew) {
            return base.Channel.ChangePassword(pUsrExtId, pOld, pNew);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> ChangePasswordAsync(long pUsrExtId, string pOld, string pNew) {
            return base.Channel.ChangePasswordAsync(pUsrExtId, pOld, pNew);
        }
        
        public System.Data.DataSet ForgotPassword(string pIde) {
            return base.Channel.ForgotPassword(pIde);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> ForgotPasswordAsync(string pIde) {
            return base.Channel.ForgotPasswordAsync(pIde);
        }
        
        public System.Data.DataSet GetAlertas(long pUsrExtId) {
            return base.Channel.GetAlertas(pUsrExtId);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> GetAlertasAsync(long pUsrExtId) {
            return base.Channel.GetAlertasAsync(pUsrExtId);
        }
        
        public System.Data.DataSet GetSenders(long pApl) {
            return base.Channel.GetSenders(pApl);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> GetSendersAsync(long pApl) {
            return base.Channel.GetSendersAsync(pApl);
        }
        
        public System.Data.DataSet GetSessionData(string pIde) {
            return base.Channel.GetSessionData(pIde);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> GetSessionDataAsync(string pIde) {
            return base.Channel.GetSessionDataAsync(pIde);
        }
        
        public System.Data.DataSet Login(string pIde, string pPsw) {
            return base.Channel.Login(pIde, pPsw);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> LoginAsync(string pIde, string pPsw) {
            return base.Channel.LoginAsync(pIde, pPsw);
        }
        
        public System.Data.DataSet LoginMobileGerencial(string pIde, string pPsw) {
            return base.Channel.LoginMobileGerencial(pIde, pPsw);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> LoginMobileGerencialAsync(string pIde, string pPsw) {
            return base.Channel.LoginMobileGerencialAsync(pIde, pPsw);
        }
        
        public System.Data.DataSet SetPersonals(long pUsrExtId, string pEmail) {
            return base.Channel.SetPersonals(pUsrExtId, pEmail);
        }
        
        public System.Threading.Tasks.Task<System.Data.DataSet> SetPersonalsAsync(long pUsrExtId, string pEmail) {
            return base.Channel.SetPersonalsAsync(pUsrExtId, pEmail);
        }
    }
}
