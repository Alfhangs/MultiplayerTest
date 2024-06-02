using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class MenuController : MonoBehaviourPunCallbacks, ILobbyCallbacks
    {
        [Header("Screen")]
        [SerializeField] private GameObject _initMenu;
        [SerializeField] private GameObject _createRoomScreen;
        [SerializeField] private GameObject _lobbyScreen;
        [SerializeField] private GameObject _lobbyNavigatorScreen;


        [Header("Init Menu")]
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _searchRoomButton;


        [Header("Lobby")]
        [SerializeField] private TextMeshProUGUI _playerListText;
        [SerializeField] private TextMeshProUGUI _inforRoomText;
        [SerializeField] private Button _playGameButton;


        [Header("Lobby Navigator")]
        [SerializeField] private RectTransform _containerRoom;
        [SerializeField] private GameObject _roomPrefab;

        private List<GameObject> _roomElemens = new List<GameObject>();
        private List<RoomInfo> _roomList = new List<RoomInfo>();

        private void Start()
        {
            _createRoomButton.interactable = false;
            _searchRoomButton.interactable = false;

            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.CurrentRoom.IsVisible = true;
                PhotonNetwork.CurrentRoom.IsOpen = true;
            }
            SetScreen(_initMenu);
        }
        
        private void SetScreen(GameObject screen)
        {
            _initMenu.SetActive(false);
            _createRoomScreen.SetActive(false);
            _lobbyScreen.SetActive(false);
            _lobbyNavigatorScreen.SetActive(false);

            screen.SetActive(true);
            if(screen == _lobbyNavigatorScreen)
            {
                UpdateLobbyNavigator();
            }
        }

        public void ChangePlayerName(TMP_InputField inputField)
        {
            PhotonNetwork.NickName = inputField.text;
        }

        public override void OnConnectedToMaster()
        {
            _createRoomButton.interactable = true;
            _searchRoomButton.interactable = true;
        }

        public void OnCreateRoomClick()
        {
            SetScreen(_createRoomScreen);
        }
        public void OnSearchRoomClick()
        {
            SetScreen(_lobbyNavigatorScreen);
        }
        public void OnBackRoomClick()
        {
            SetScreen(_initMenu);
        }

        public void OnCreateRoomButton(TMP_InputField name)
        {
            NetworkManager.instance.CreateRoom(name.text);
        }

        public override void OnJoinedRoom()
        {
            SetScreen(_lobbyScreen);
            photonView.RPC("UpdateLobby", RpcTarget.All);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            UpdateLobby();
        }

        [PunRPC]
        private void UpdateLobby()
        {
            _playGameButton.interactable = PhotonNetwork.IsMasterClient;

            _playerListText.text = "";

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                _playerListText.text += player.NickName + "\n";
            }
            _inforRoomText.text = string.Format(@"<b>Room Name: </b> {0}{1}","\n",PhotonNetwork.CurrentRoom.Name);
        }

        public void OnPlayGameClick()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
        }

        public void OnExitLobby()
        {
            PhotonNetwork.LeaveRoom();
            SetScreen(_initMenu);
        }

        private GameObject CreateRoomButton()
        {
            GameObject gameObject = Instantiate(_roomPrefab, _containerRoom.transform);
            _roomElemens.Add(gameObject);
            return gameObject;
        }

        void UpdateLobbyNavigator()
        {
            foreach (GameObject prefab in _roomElemens)
            {
                prefab.SetActive(false);
            }

            for (int i = 0; i < _roomList.Count; i++)
            {
                GameObject prefab = i >= _roomElemens.Count ? CreateRoomButton() : _roomElemens[i];
                prefab.SetActive(true);
            }
        }

        public void OnRefreshClick()
        {
            UpdateLobbyNavigator();
        }

        private void OnJoinRoomClick(string name)
        {
            NetworkManager.instance.JoinRoom(name); 
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            _roomList = roomList;
        }

        public void LoadMenu()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}