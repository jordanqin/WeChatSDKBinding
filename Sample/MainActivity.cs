using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Com.Tencent.MM.Sdk.Openapi;
using Com.Tencent.MM.Sdk.Modelbase;
using Com.Tencent.MM.Sdk.Constants;
using Com.Tencent.MM.Sdk.Modelmsg;
using Java.Lang;

namespace Sample
{
    [Activity(Label = "Sample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IWXAPIEventHandler
    {
        // IWXAPI 是第三方app和微信通信的openapi接口
        private IWXAPI api;
        //最小支持朋友圈的版本
        private const int TIMELINE_SUPPORTED_VERSION = 0x21020001;

        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // 通过WXAPIFactory工厂，获取IWXAPI的实例
            api = WXAPIFactory.CreateWXAPI(this, Configuration.APP_ID, false);

            // Get our button from the layout resource,
            // and attach an event to it
            Button butRegisterApp = FindViewById<Button>(Resource.Id.butRegisterApp);
            butRegisterApp.Click += butRegisterApp_Click;
            Button butSendTo = FindViewById<Button>(Resource.Id.butSendTo);
            butSendTo.Click += butSendTo_Click;
            Button butOpenWXApp = FindViewById<Button>(Resource.Id.butOpenWXApp);
            butOpenWXApp.Click += butOpenWXApp_Click;
            Button butIsFriendster = FindViewById<Button>(Resource.Id.butIsFriendster);
            butIsFriendster.Click += butIsFriendster_Click;
        }

        void butIsFriendster_Click(object sender, EventArgs e)
        {
            int wxSdkVersion = api.WXAppSupportAPI;
            if (wxSdkVersion >= TIMELINE_SUPPORTED_VERSION)
            {
                Toast.MakeText(this, "wxSdkVersion = " + wxSdkVersion + "\n支持", ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(this, "wxSdkVersion = " + wxSdkVersion + "\n不支持", ToastLength.Long).Show();
            }
        }

        void butOpenWXApp_Click(object sender, EventArgs e)
        {
            Toast.MakeText(this, "launch result = " + api.OpenWXApp(), ToastLength.Long).Show();
        }

        void butSendTo_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(this, typeof(SendToWXActivity)));
		        Finish();
        }

        void butRegisterApp_Click(object sender, EventArgs e)
        {
            // 将该app注册到微信
            api.RegisterApp(Configuration.APP_ID);
        }

	protected override void OnNewIntent(Intent intent) 
    {
		base.OnNewIntent(intent);
		this.Intent=intent;
        api.HandleIntent(intent, this);
	}

	// 微信发送请求到第三方应用时，会回调到该方法
    public void OnReq(BaseReq req) 
    {
		switch (req.Type)
        {
		case ConstantsAPI.CommandGetmessageFromWx:
			goToGetMsg();		
			break;
		case ConstantsAPI.CommandShowmessageFromWx:
			goToShowMsg((ShowMessageFromWX.Req) req);
			break;
		default:
			break;
		}
	}

	// 第三方应用发送到微信的请求处理后的响应结果，会回调到该方法
    public void OnResp(BaseResp resp) 
    {
		int result = 0;
		
		switch (resp.MyErrCode) {
		case BaseResp.ErrCode.ErrOk:
			result = Resource.String.errcode_success;
			break;
		case BaseResp.ErrCode.ErrUserCancel:
			result = Resource.String.errcode_cancel;
			break;
		case BaseResp.ErrCode.ErrAuthDenied:
			result = Resource.String.errcode_deny;
			break;
		default:
			result = Resource.String.errcode_unknown;
			break;
		}
		
		Toast.MakeText(this, result, ToastLength.Long).Show();
	}
	
	private void goToGetMsg() 
    {
		Intent intent = new Intent(this, typeof(GetFromWXActivity));
		intent.PutExtras(this.Intent);
		StartActivity(intent);
		Finish();
	}
	
	private void goToShowMsg(ShowMessageFromWX.Req showReq)
    {
		WXMediaMessage wxMsg = showReq.Message;		
		WXAppExtendObject obj = (WXAppExtendObject) wxMsg.MyMediaObject;
		
        // 组织一个待显示的消息内容
		StringBuffer msg = new StringBuffer(); 
		msg.Append("description: ");
		msg.Append(wxMsg.Description);
		msg.Append("\n");
		msg.Append("extInfo: ");
		msg.Append(obj.ExtInfo);
		msg.Append("\n");
		msg.Append("filePath: ");
		msg.Append(obj.FilePath);
		
		Intent intent = new Intent(this,typeof(ShowFromWXActivity));
        intent.PutExtra(Configuration.ShowMsgActivity.STitle, wxMsg.Title);
        intent.PutExtra(Configuration.ShowMsgActivity.SMessage, msg.ToString());
        //intent.PutExtra(Configuration.ShowMsgActivity.BAThumbData, wxMsg.ThumbData);
		StartActivity(intent);
		Finish();
	}
    }
}

