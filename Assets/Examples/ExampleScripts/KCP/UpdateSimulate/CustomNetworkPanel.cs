﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cosmos;
using Cosmos.UI;
using Cosmos.Network;
using System.IO;
namespace Cosmos.Test
{
    public class CustomNetworkPanel : UIResidentForm
    {
        Button btnConnect;
        Button btnDisconnect;
        InputField inputMsg;

        protected override void OnInitialization()
        {
            btnConnect = GetUIForm<Button>("BtnConnect");
            btnConnect.onClick.AddListener(ConnectClick);
            btnDisconnect = GetUIForm<Button>("BtnDisconnect");
            btnDisconnect.onClick.AddListener(DisconnectClick);
            inputMsg = GetUIForm<InputField>("InputMsg");
        }
        void ConnectClick()
        {
            CosmosEntry.NetworkManager.Connect(KCPNetwork.Instance.IP, KCPNetwork.Instance.Port, NetworkProtocolType.UDP);
            //KCPNetwork.Instance.Connect();
        }
        void DisconnectClick()
        {
            CosmosEntry.NetworkManager.Disconnect();
            //KCPNetwork.Instance.Disconnect();
        }
        protected override void OnTermination()
        {
            CosmosEntry.NetworkManager.Disconnect();
        }
    }
}