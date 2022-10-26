using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class PersonaBuzon : MonoBehaviour
{
    DatabaseReference _mDatabase;
    public Text txtPersona;
    public string nombre;
    public string remitente;
    public string useridNombre;
    void Start()
    {
        txtPersona.text = nombre;
        
        _mDatabase = FirebaseDatabase.DefaultInstance.RootReference;
    }   

    public void SetAmigos()
    {
        try
        {
            UserData data = new UserData();
            data.username = nombre;
            string json = JsonUtility.ToJson(data);
            _mDatabase.Child("users").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("amigos").Child(nombre).SetRawJsonValueAsync(json);
            UserData data2 = new UserData();
            data.username = remitente;
            string json2 = JsonUtility.ToJson(data2);
            _mDatabase.Child("users").Child(useridNombre).Child("amigos").Child(remitente).SetRawJsonValueAsync(json2);
            
            try
            {
                _mDatabase.Child("buzones").Child(remitente).Child(nombre).SetRawJsonValueAsync(null);
                Destroy(gameObject);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            
            Destroy(gameObject);
            
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    public void DeleteAmigos()
    {
        try
        {
            _mDatabase.Child("buzones").Child(remitente).Child(nombre).SetRawJsonValueAsync(null);
            Destroy(gameObject);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
