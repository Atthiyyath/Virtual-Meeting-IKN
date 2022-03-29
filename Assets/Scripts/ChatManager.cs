using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using TMPro;
using Photon.Pun;

public class ChatManager : MonoBehaviour
{
    private PhotonView photonView;
    private List<string> messages = new List<string>();
    private float buildDelay = 0f;
    private int maximumMessages = 14;

    public TMP_InputField chatInput;
    public TextMeshProUGUI chatContent;

    void Start() {
        photonView = GetComponent<PhotonView>();
    }

    void Update() {
        if (PhotonNetwork.InRoom) {
            chatContent.maxVisibleLines = maximumMessages;
            if (messages.Count > maximumMessages) {
                messages.RemoveAt(0);
            }
            if (buildDelay < Time.time) {
                BuildChatContents();
                buildDelay = Time.time + 0.25f;
            }
        } else if (messages.Count > 0) {
            messages.Clear();
            chatContent.text = "";
        }

        if (Keyboard.current.enterKey.wasReleasedThisFrame) {
            if (EventSystem.current.currentSelectedGameObject == chatInput) {
                //Debug.Log("Test 1");
                EventSystem.current.SetSelectedGameObject(chatInput.gameObject, null);
                chatInput.OnPointerClick(new PointerEventData(EventSystem.current));
            } else {
                //Debug.Log("Test 2");
                SubmitChat();
            }
        }
    }

    [PunRPC]
    void RPC_AddNewMessage(string msg) {
        messages.Add(msg);
    }

    public void SendChat(string msg) {
        string newMessage = PhotonNetwork.NickName + ": " + msg;
        photonView.RPC("RPC_AddNewMessage", RpcTarget.All, newMessage);
    }

    public void SubmitChat() {
        string blankCheck = chatInput.text;
        blankCheck = Regex.Replace(blankCheck, @"\s", "");
        if (blankCheck == "") {
            chatInput.DeactivateInputField();
            chatInput.text = "";
            return;
        }

        SendChat(chatInput.text);
        chatInput.DeactivateInputField();
        chatInput.text = "";
    }

    void BuildChatContents() {
        string newContents = "";
        foreach (string s in messages) {
            newContents += s + "\n";
        }
        chatContent.text = newContents;
    }
}
