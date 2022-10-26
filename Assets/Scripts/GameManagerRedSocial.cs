using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class GameManagerRedSocial : MonoBehaviour
{
    DatabaseReference _mDatabase;

    [SerializeField] public GameObject Notification;
    [SerializeField] private Text _txtNotificacion;

    [SerializeField] private GameObject personaOnline;
    [SerializeField] private Transform padreOnline;

    [SerializeField] private GameObject personaBuzon;
    [SerializeField] private Transform padreBuzon;

    [SerializeField] private GameObject personaAmiga;
    [SerializeField] private Transform padreAmiga;
    private string _username;
    private int countSalas = 0;

    private int stateMatch = 0;
    private int countPlayersSala = 0;
    private  string nombreSala;
    // Start is called before the first frame update


    public void ClickCerrarNotificacion()
    {
        Notification.SetActive(false);
    }

    private void GetName(object sender, EventArgs e)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users/" + FirebaseAuth.DefaultInstance.CurrentUser.UserId + "/username")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    _username = (string) snapshot.Value;
                    //Debug.Log(_username);
                    if (_username != null)
                    {
                        SetOnline();
                        var _databaseBuzon = FirebaseDatabase.DefaultInstance.GetReference("/buzones/").Child(_username);
                        _databaseBuzon.ChildAdded += HandleChildAddedBuzon;
                        
                    }
                }
            });

    }

    private void Start()
    {
        
        countSalas = 0;
        stateMatch = 0;
        countPlayersSala = 0;
    }

    private void Awake()
    {
        
        FirebaseAuth.DefaultInstance.StateChanged += GetName;
        _mDatabase = FirebaseDatabase.DefaultInstance.RootReference;
        var _databaseOnline = FirebaseDatabase.DefaultInstance.GetReference("/online/");
        _databaseOnline.ChildAdded += HandleChildAddedOnline;
        
        var _databaseAmigos =
            FirebaseDatabase.DefaultInstance.GetReference("users/" + FirebaseAuth.DefaultInstance.CurrentUser.UserId +
                                                          "/amigos/");
        _databaseAmigos.ChildAdded += HandleChildAddedAmigos;

        var _databaseOnlineRemoved = FirebaseDatabase.DefaultInstance.GetReference("/online/");
        _databaseOnlineRemoved.ChildRemoved += HandleChildOnlineRemoved;
        

        
        
        
        
        

    }

    public void SetOnline()
    {
        try
        {
            UserData data = new UserData();
            data.username = _username;
            string json = JsonUtility.ToJson(data);
            _mDatabase.Child("online").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
                .SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void CerrarSesion()
    {
        _mDatabase.Child("online").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).SetRawJsonValueAsync(null);
        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene(0);

    }

    /*-------------------------HAndles----------------------------------------------*/
    void HandleChildAddedOnline(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        // Do something with the data in args.Snapshot
        try
        {
            //Debug.Log(args.Snapshot);
            Dictionary<string, object> users = (Dictionary<string, object>) args.Snapshot.Value;
            //Debug.Log(users["username"]);
            personaOnline.GetComponent<PersonaOnline>().nombre = (string) users["username"];
            personaOnline.GetComponent<PersonaOnline>().remitente = _username;
            personaOnline.name = args.Snapshot.Key;
            Instantiate(personaOnline).transform.SetParent(padreOnline.transform);



            _txtNotificacion.text = users["username"] + " se ha conectado.";
            Notification.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }


    }

    void HandleChildAddedBuzon(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        // Do something with the data in args.Snapshot
        try
        {
            //Debug.Log(args.Snapshot);
            Debug.Log(args.Snapshot.Key);
            Dictionary<string, object> users = (Dictionary<string, object>) args.Snapshot.Value;
            personaBuzon.GetComponent<PersonaBuzon>().nombre =users["name"].ToString();
            personaBuzon.GetComponent<PersonaBuzon>().remitente = _username;
            personaBuzon.GetComponent<PersonaBuzon>().useridNombre = users["username"].ToString();
            Instantiate(personaBuzon).transform.SetParent(padreBuzon.transform);



        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }


    }

    void HandleChildAddedAmigos(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        // Do something with the data in args.Snapshot
        try
        {
            //Debug.Log(args.Snapshot);
            personaAmiga.GetComponent<PersonaAmiga>().nombre = args.Snapshot.Key;

            Instantiate(personaAmiga).transform.SetParent(padreAmiga.transform);


        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }


    }

    void HandleChildOnlineRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        try
        {
            Debug.Log(args.Snapshot);
            Dictionary<string, object> users = (Dictionary<string, object>) args.Snapshot.Value;
            var gameObject = GameObject.Find((string) args.Snapshot.Key + "(Clone)");
            Debug.Log(args.Snapshot.Key);
            Destroy(gameObject);
            _txtNotificacion.text = users["username"] + " se ha desconectado";
            Notification.SetActive(true);
            // Do something with the data in args.Snapshot
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }
    
    
    /*-------------------------------------------------Aqui va Matchmaking----------------------------------------*/

    void HandleGetSalas(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        try
        {
            
            countSalas += 1;
            if (countSalas > 0)
            {
                nombreSala = args.Snapshot.Key;
                Debug.Log(args.Snapshot);
                JoinSala(args.Snapshot.Key);
                ActivateHandlePlayers(args.Snapshot.Key);
                Debug.Log("Numero de salas  :  " + countSalas);
                countSalas = 0;
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }void HandleGetPlayersSala(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        try
        {
            Debug.Log(args.Snapshot);
            countPlayersSala += 1;
            Debug.Log("Numero de personas en sala  :  " + countPlayersSala);
            if (countPlayersSala >= 2)
            {
                CrearJuego(nombreSala);
                SceneManager.LoadScene(3);
                countPlayersSala = 0;
            }
            

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }

    public void BtnMatch()
    {

        if (stateMatch == 0)
        {
            ActivateHandle();
            stateMatch = 1;
        }

        else if (stateMatch == 1)
        {
            stateMatch = 2;
            CreateSala();
        }
        else
        {
            Debug.Log("cagamos");
        }

    }

    public void CreateSala()
    {
        UserData data = new UserData();
        data.username = _username;
        data.name = "EnSala";
        string json = JsonUtility.ToJson(data);
        _mDatabase.Child("Salas").Child("Sala"+_username).Child(_username).SetRawJsonValueAsync(json);

    }
    public void CrearJuego(string nameSala)
    {
        DataUserGame data = new DataUserGame();
        data.username = _username;
        data.score = 0;
        string json = JsonUtility.ToJson(data);
        _mDatabase.Child("juegos").Child(nameSala).Child(_username).SetRawJsonValueAsync(json);
        _mDatabase.Child("Salas").Child(nameSala).SetRawJsonValueAsync(null);

    }
    
    public void JoinSala(string nameSala)
    {
        UserData data = new UserData();
        data.username = _username;
        data.name = "EnSala";
        string json = JsonUtility.ToJson(data);
        _mDatabase.Child("Salas").Child(nameSala).Child(_username).SetRawJsonValueAsync(json);

    }

    public void ActivateHandle()
    {
        var _databaseGetSalas = FirebaseDatabase.DefaultInstance.GetReference("/Salas/");
        _databaseGetSalas.ChildAdded += HandleGetSalas;
    }
    public void ActivateHandlePlayers(string sala)
    {
        var _databaseGetPlayersSala = FirebaseDatabase.DefaultInstance.GetReference("/Salas/"+sala);
        _databaseGetPlayersSala.ChildAdded += HandleGetPlayersSala;
    }


}

public class DataUserGame
{
    public string username;
    public int score;
}

