using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Analytics;
//using Firebase.Firestore.DocumentReference;
using TMPro;

public class AuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBReference;
    //public DocumentReference DocumentReference;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public string copyEmailId = "";
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    //public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;
    public TMP_Text confirmRegisterText;

    [Header("MenuScreen")]
    public TMP_Text warningMenuText;
    public TMP_Text confirmMenuText;

    [Header("Create Details Screens")]
    public GameObject Profile;
    public GameObject About;
    public GameObject Products;
    public GameObject Contact;
    public GameObject Privacy;

    [Header("UserCreatePofileData")]
    public TMP_InputField name;
    public TMP_InputField age;
    public TMP_InputField profession;
    public TMP_InputField experience;
    public TMP_Text confirmCreateProfileText;
    public TMP_Text warningCreateProfileText;

    [Header("UserCreateAboutData")]
    public TMP_InputField about;

    [Header("UserCreateProductsData")]
    public TMP_InputField product1;
    public TMP_InputField product2;
    public TMP_InputField product3;

    [Header("UserCreateContactData")]
    public TMP_InputField phone;
    public TMP_InputField mail;
    public TMP_InputField address;
    public TMP_InputField website;

    [Header("UserCreatePrivacyData")]
    public Toggle publicToggler;
    private bool publicaccess = true;
    public GameObject CreatePrivacyPrivateObject;
    public TMP_InputField privatemails;
    public TMP_Text warningCreatePrivacyText;
    public TMP_Text confirmCreatePrivacyText;


    [Header("UserUpdateData")]
    public TMP_InputField updateName;
    public TMP_InputField updateAge;
    public TMP_InputField updateProfession;
    public TMP_InputField updateExperience;
    public TMP_Text confirmUpdateText;
    public TMP_Text warningUpdateText;
    public GameObject UpdateProfileScene;

    public GameObject UpdatePrivacyScene;
    public Toggle updatePrivacyToggler;
    private bool updatePrivacyAccess = true;
    public GameObject UpdatePrivacyPrivateObject;
    public TMP_InputField updateprivatemails;
    public TMP_Text warningUpdatePrivacyText;
    public TMP_Text confirmUpdatePrivacyText;

    [Header("UserDeleteData")]
    public TMP_InputField passwordDeleteField;
    public TMP_Text warningDeleteText;
    public TMP_Text confirmDeleteText;



    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBReference = FirebaseDatabase.DefaultInstance.RootReference;
    }


    ///////////////////////////////////////////////////        WELCOME_UI       ////////////////////////////////////////////////////

    

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text
            //, usernameRegisterField.text
        ));
    }


    //2nd Screen Buttons
    public void SignOutButton()
    {
        auth.SignOut();

        ClearLogInFields();
        ClearRegisterFields();
        copyEmailId = "";

        StartCoroutine(SigningOutDelay());
    }
    public IEnumerator SigningOutDelay() 
    {
        confirmMenuText.text = "Signing Out...";
        yield return new WaitForSeconds(1);
        confirmMenuText.text = "";
        UIManager.instance.WelcomeScreen(); 
    }


    ///////////////////////////////////////////////////////    MENU_UI  BUTTONS   /////////////////////////////////////////////////

    public void MenuToCreateButon()
    {
        StartCoroutine(IsCardCreated());
    }

    public IEnumerator IsCardCreated()
    {
        string key = getNodeForDB();
        var DBTask = DBReference.Child("users").Child(key).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to update task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            UIManager.instance.CreateScreen();
        }
        else
        {
            Debug.Log("Card exists!!");
            warningMenuText.text = "You have already created a Card";
            yield return new WaitForSeconds(2);
            warningMenuText.text = "";
        }
    }

    ///////////////////////////////////////////////////////    MENU_UI  CREATE    /////////////////////////////////////////////////



    public void CreateProfileNextButton()
    {

        //check if name is valid
        if (!string.IsNullOrEmpty(name.text))
        {
            //check other inputs too and then insert all coroutines in here which r listed below
            if (!string.IsNullOrEmpty(age.text))
            {
                if (!string.IsNullOrEmpty(profession.text))
                {
                    if (!string.IsNullOrEmpty(experience.text))
                    {
                        Profile.SetActive(false);
                        About.SetActive(true);
                        warningCreateProfileText.text = "";
                    }
                    else
                    {
                        warningCreateProfileText.text = "Experience can't be empty";
                    }
                }
                else
                {
                    warningCreateProfileText.text = "Profession can't be empty";
                }
            }
            else
            {
                warningCreateProfileText.text = "Age can't be empty";
            }
        }
        else
        {
            warningCreateProfileText.text = "Name can't be empty"; 
        }

    }

    public void CreateToMenu()
    {
        ClearCreateFields();
        warningCreateProfileText.text = "";
        confirmCreateProfileText.text = "";
        UIManager.instance.MenuScreen();
    }


    public void CreateAboutNextButton()
    {
        About.SetActive(false);
        Products.SetActive(true);
    }

    public void CreateAboutToProfile()
    {
        About.SetActive(false);
        Profile.SetActive(true);
    }


    public void CreateProductsNextButton()
    {
        Products.SetActive(false);
        Contact.SetActive(true);
    }

    public void CreateProductsToAboutButton()
    {
        Products.SetActive(false);
        About.SetActive(true);
    }

    public void CreateContactToProductsButton()
    {
        Contact.SetActive(false);
        Products.SetActive(true);
    }

    public void CreateContactNextButton()
    {
        Contact.SetActive(false);
        Privacy.SetActive(true);
    }

    public void CreatePrivacyCreateButton()
    {
        StartCoroutine(UpdateEmailAuth(copyEmailId));

        //Updating email with conditions (remove last @ and .   then replace all . with : )
        //use same logic while scanning and checking in db
        string key;
        key = getNodeForDB();

        //Profile
        StartCoroutine(UpdateEmailDatabase(key, copyEmailId));
        StartCoroutine(UpdateNameDatabase(key, name.text));
        StartCoroutine(UpdateAgeDatabase(key, int.Parse(age.text)));
        StartCoroutine(UpdateProfessionDatabase(key, profession.text));
        StartCoroutine(UpdateExperienceDatabase(key, int.Parse(experience.text)));

        //About
        StartCoroutine(UpdateAboutDatabase(key, about.text));

        //Products
        StartCoroutine(UpdataProductsDatabase(key, product1.text, product2.text, product3.text));

        //Contact
        StartCoroutine(UpdateContactPhoneDatabase(key, phone.text));
        StartCoroutine(UpdateContactMailDatabase(key, mail.text));
        StartCoroutine(UpdateContactAddressDatabase(key, address.text));
        StartCoroutine(UpdateContactWebsiteDatabase(key, website.text));

        //Privacy
        Debug.Log(publicToggler.GetComponent<Toggle>().isOn);
        publicaccess = publicToggler.GetComponent<Toggle>().isOn;

        StartCoroutine(UpdatePrivacyAccessDatabase(key, publicaccess));

        string finalmails = FormattedEmails(privatemails.text);
    
        StartCoroutine(UpdatePrivacyMailsDatabase(key, finalmails));
        
        StartCoroutine(SuccessCreated());
    }

    public void CreatePrivacyToContactButton()
    {
        Privacy.SetActive(false);
        Contact.SetActive(true);
    }

    public void privateObjectToggler(bool toggle)
    {
        if (toggle)
            CreatePrivacyPrivateObject.SetActive(false);
        else
            CreatePrivacyPrivateObject.SetActive(true);
    }

    public IEnumerator SuccessCreated()
    {
        warningCreatePrivacyText.text = "";
        confirmCreatePrivacyText.text = "Created Successfully";
        yield return new WaitForSeconds(2);

        confirmCreatePrivacyText.text = "Upload your card pic";
        yield return new WaitForSeconds(2);

        ClearCreateFields();
        UIManager.instance.MenuScreen();
        confirmCreatePrivacyText.text = "";

    }

    public void ClearCreateFields()
    {
        name.text = "";
        age.text = "";
        profession.text = "";
        experience.text = "";
    }

    ////////////////////////////////////////////////////    MENU_UI UPDATE    /////////////////////////////////////////////////////

    public void MenuToUpdateButton()
    {
        //go to update screen
        UIManager.instance.UpdateScreen();
        warningUpdateText.text = "";
        confirmUpdateText.text = "";
        //load data
        StartCoroutine(LoadUserProfileData());
        
    }

    private IEnumerator LoadUserProfileData()
    {
        string key;
        key = getNodeForDB();

        //Get the currently logged in user data
        var DBTask = DBReference.Child("users").Child(key).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to update task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            updateName.text = "";
            updateAge.text = "";
            updateProfession.text = "";
            updateExperience.text = "";
            warningUpdateText.text = "Create your card to update details";
            confirmUpdateText.text = "";
        }
        else
        {
            Debug.Log("Loading user data!!");
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            updateName.text = snapshot.Child("profilename").Value.ToString();
            updateAge.text = snapshot.Child("profileage").Value.ToString();
            updateProfession.text = snapshot.Child("profileprofession").Value.ToString();
            updateExperience.text = snapshot.Child("profileexperience").Value.ToString();
        }
    }

    public void UpdateProfileNextButton()
    {
        //check if name is valid
        if (!string.IsNullOrEmpty(updateName.text))
        {
            //check other inputs too and then insert all coroutines in here which r listed below
            if (!string.IsNullOrEmpty(updateAge.text))
            {
                if (!string.IsNullOrEmpty(updateProfession.text))
                {
                    if (!string.IsNullOrEmpty(updateExperience.text))
                    {
                        UpdateProfileScene.SetActive(false);
                        UpdatePrivacyScene.SetActive(true);
                        //LoadUserPrivacyData();
                    }
                    else
                    {
                        warningUpdateText.text = "Experience can't be empty";
                    }
                }
                else
                {
                    warningUpdateText.text = "Profession can't be empty";
                }
            }
            else
            {
                warningUpdateText.text = "Age can't be empty";
            }
        }
        else
        {
            warningUpdateText.text = "Name can't be empty";
        } 
    }

    public void updatePrivateObjectToggler(bool toggle)
    {
        if (toggle)

            UpdatePrivacyPrivateObject.SetActive(false);
        else
            UpdatePrivacyPrivateObject.SetActive(true);
    }

    public IEnumerator LoadUserPrivacyData()
    {
        string key;
        key = getNodeForDB();

        //Get the currently logged in user data
        var DBTask = DBReference.Child("users").Child(key).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to update task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
        }
        else
        {
            Debug.Log("Loading user privacy data!!");
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            updateprivatemails.text = snapshot.Child("accessto").Value.ToString();
        }
    }

    public void UpdateToMenuButton()
    {
        ClearUpdateFields();
        warningUpdateText.text = "";
        confirmUpdateText.text = "";
        UIManager.instance.MenuScreen();
    }

    public void UpdatePrivacySubmitButton()
    {
        StartCoroutine(UpdateEmailAuth(copyEmailId));
        //Updating email with conditions (remove last @ and .   then replace all . with : )................................
        //use same logic while scanning and checking in db
        string key;
        key = getNodeForDB();

        StartCoroutine(UpdateEmailDatabase(key, copyEmailId));
        StartCoroutine(UpdateNameDatabase(key, updateName.text));
        StartCoroutine(UpdateAgeDatabase(key, int.Parse(updateAge.text)));
        StartCoroutine(UpdateProfessionDatabase(key, updateProfession.text));
        StartCoroutine(UpdateExperienceDatabase(key, int.Parse(updateExperience.text)));

        updatePrivacyAccess = updatePrivacyToggler.GetComponent<Toggle>().isOn;
        StartCoroutine(UpdatePrivacyAccessDatabase(key, updatePrivacyAccess));

        string finalmails = FormattedEmails(updateprivatemails.text);
        StartCoroutine(UpdatePrivacyMailsDatabase(key, finalmails));

        StartCoroutine(SuccessUpdated());

    }

    public void UpdatePrivacyToProfileButton()
    {
        UpdatePrivacyScene.SetActive(false);
        UpdateProfileScene.SetActive(true);
    }

    public IEnumerator SuccessUpdated()
    {
        warningUpdatePrivacyText.text = "";
        confirmUpdatePrivacyText.text = "Updated Successfully";
        yield return new WaitForSeconds(2);

        ClearUpdateFields();
        UIManager.instance.MenuScreen();
        confirmUpdatePrivacyText.text = "";
    }

    public void ClearUpdateFields()
    {
        updateName.text = "";
        updateAge.text = "";
        updateProfession.text = "";
        updateExperience.text = "";
    }

    ////////////////////////////////////////////////////    MENU_UI DELETE    /////////////////////////////////////////////////////

    public void DeleteButton()
    {
        if (!string.IsNullOrEmpty(passwordDeleteField.text))
        {
            StartCoroutine(DeleteCard(passwordDeleteField.text));
            //DeleteToMenuButton();
        }
        else
        {
            warningDeleteText.text = "Enter Password";
        }
    }

    public void DeleteToMenuButton()
    {
        warningDeleteText.text = "";
        confirmDeleteText.text = "";
        passwordDeleteField.text = "";
        UIManager.instance.MenuScreen();
    }


    public IEnumerator DeleteCard(string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var DeleteTask = auth.SignInWithEmailAndPasswordAsync(copyEmailId, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => DeleteTask.IsCompleted);

        if (DeleteTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {DeleteTask.Exception}");
            FirebaseException firebaseEx = DeleteTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningDeleteText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = DeleteTask.Result;
            var DBTask = DBReference.Child("users").Child(getNodeForDB()).RemoveValueAsync();

            Debug.LogFormat("Card with email {0} deleted successfully", User.Email);
         
            warningDeleteText.text = "";
            confirmDeleteText.text = "Card Deleted Successfully";

            yield return new WaitForSeconds(1);
            UIManager.instance.MenuScreen();
            confirmDeleteText.text = "";

            passwordDeleteField.text = "";

        }
    }





    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ",User.Email);
            copyEmailId = User.Email;
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            var k = getNodeForDB();
            UnityMediaPicker.AssignKey(k);
            Debug.Log("Setting key in UnityMediaPicker class ===>" + UnityMediaPicker._key);

            yield return new WaitForSeconds(1);
            UIManager.instance.MenuScreen();
            confirmLoginText.text = "";

            ClearLogInFields();
            ClearRegisterFields();

        }
    }

    private IEnumerator Register(string _email, string _password)
    {

        if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _email };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        Debug.LogFormat("User registered successfully: {0} ", User.Email);

                        warningRegisterText.text = "";
                        confirmRegisterText.text = "Registered Successfully";
                        yield return new WaitForSeconds(1);

                        UIManager.instance.WelcomeScreen();
                        confirmRegisterText.text = "";

                        ClearLogInFields();
                        ClearRegisterFields();

                    }
                }
            }
        }
    }

    public void ClearLogInFields()
    {
        //Clear LoginFields
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }

    public void ClearRegisterFields()
    {
        //Clear RegisterFields
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    public string getNodeForDB()
    {
        string key = string.Empty;
        string keycopy = "";
        string delete = ".";

        //reversing emailid
        for (int i = copyEmailId.Length - 1; i >= 0; i--)
        {
            key += copyEmailId[i];
        }

        //removing @ and last .
        for (int i = 0; i < key.Length; i++)
        {
            if (key[i] == System.Convert.ToChar(delete))
            {
                delete = "@";
            }
            else
            {
                keycopy += key[i];
            }
        }
        key = string.Empty;

        //reversing keycopy
        for (int i = keycopy.Length - 1; i >= 0; i--)
        {
            key += keycopy[i];
        }

        key = key.Replace(".", ":");

        return key;
    }

    private string FormattedEmails(string privateEmails)
    {

        string[] allmails = privateEmails.Split(new char[] { ',' });
        List<string> list = new List<string>();
        string finalmails = "";

        foreach (string mail in allmails)
        {
            string tempkey = string.Empty;
            string keycopy = "";
            string delete = ".";

            //reversing emailid
            for (int i = mail.Length - 1; i >= 0; i--)
            {
                tempkey += mail[i];
            }

            //removing @ and last .
            for (int i = 0; i < tempkey.Length; i++)
            {
                if (tempkey[i] == System.Convert.ToChar(delete))
                {
                    delete = "@";
                }
                else
                {
                    keycopy += tempkey[i];
                }
            }
            tempkey = string.Empty;

            //reversing keycopy
            for (int i = keycopy.Length - 1; i >= 0; i--)
            {
                tempkey += keycopy[i];
            }

            tempkey = tempkey.Replace(".", ":");

            list.Add(tempkey);
        }

        finalmails = String.Join(",", list.ToArray());
        return finalmails;
    }

    private IEnumerator UpdateEmailAuth(string _email)
    {
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _email };

        //Call the Firebase auth update user profile function passing the profile with the username
        var ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }
    }

    private IEnumerator UpdateEmailDatabase(string key, string _email)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBReference.Child("users").Child(key).Child("email").SetValueAsync(_email);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    private IEnumerator UpdateNameDatabase(string key, string _name)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBReference.Child("users").Child(key).Child("profilename").SetValueAsync(_name);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    private IEnumerator UpdateAgeDatabase(string key, int _age)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBReference.Child("users").Child(key).Child("profileage").SetValueAsync(_age);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    private IEnumerator UpdateProfessionDatabase(string key, string _profession)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBReference.Child("users").Child(key).Child("profileprofession").SetValueAsync(_profession);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    private IEnumerator UpdateExperienceDatabase(string key, int _experience)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBReference.Child("users").Child(key).Child("profileexperience").SetValueAsync(_experience);
            
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }


    public IEnumerator UpdateAboutDatabase(string key, string about)
    {
        var DBTask = DBReference.Child("users").Child(key).Child("about").SetValueAsync(about);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }


    public IEnumerator UpdataProductsDatabase(string key, string p1, string p2, string p3)
    {
        var DBTask = DBReference.Child("users").Child(key).Child("productp1").SetValueAsync(p1);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }

        DBTask = DBReference.Child("users").Child(key).Child("productp2").SetValueAsync(p2);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        DBTask = DBReference.Child("users").Child(key).Child("productp3").SetValueAsync(p3);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    }


    public IEnumerator UpdateContactPhoneDatabase(string key, string phone)
    {
        var DBTask = DBReference.Child("users").Child(key).Child("contactphone").SetValueAsync(phone);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }

    }

    public IEnumerator UpdateContactMailDatabase(string key, string mail)
    {
        var DBTask = DBReference.Child("users").Child(key).Child("contactmail").SetValueAsync(mail);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }

    }

    public IEnumerator UpdateContactAddressDatabase(string key, string address)
    {
        var DBTask = DBReference.Child("users").Child(key).Child("contactaddress").SetValueAsync(address);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }

    }

    public IEnumerator UpdateContactWebsiteDatabase(string key, string website)
    {
        var DBTask = DBReference.Child("users").Child(key).Child("contactwebsite").SetValueAsync(website);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    public IEnumerator UpdatePrivacyAccessDatabase(string key, bool value)
    {
        var DBTask = DBReference.Child("users").Child(key).Child("public").SetValueAsync(value);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    public IEnumerator UpdatePrivacyMailsDatabase(string key, string mails)
    {
        var DBTask = DBReference.Child("users").Child(key).Child("accessto").SetValueAsync(mails);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

}
