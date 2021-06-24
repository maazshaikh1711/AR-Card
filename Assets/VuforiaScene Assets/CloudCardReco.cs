using UnityEngine;
using System;
using System.Collections;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Vuforia;
using UnityEngine.SceneManagement;
using TMPro;

public class CloudCardReco : MonoBehaviour
{
	public ImageTargetBehaviour ImageTargetTemplate;
	private CloudRecoBehaviour mCloudRecoBehaviour;
	private bool mIsScanning = true;
	private string mTargetMetadata = "";
	//public static UIManager instance;


	//Firebase variables
	[Header("Firebase")]
	public DependencyStatus dependencyStatus;
	public FirebaseAuth auth;
	public FirebaseUser User;
	public DatabaseReference DBReference;


	[Header("Canvas")]
	public GameObject LoginPage;
	public GameObject CardCanvas;

	//Login variables
	[Header("Login")]
	public TMP_InputField emailLoginField;
	public TMP_InputField passwordLoginField;
	public TMP_Text warningLoginText;
	public TMP_Text confirmLoginText;

	[Header("Card Details")]
	//Profile
	public TMP_Text TitleName;
	public TMP_Text TitleProfession;
	public TMP_Text ProfileName;
	public TMP_Text ProfileAge;
	public TMP_Text ProfileProfession;
	public TMP_Text ProfileExperience;

	//About
	public TMP_Text Aboutabout;

	//Product
	public TMP_Text Product1;
	public TMP_Text Product2;
	public TMP_Text Product3;

	//Contact
	public TMP_Text ContactPhone;
	public TMP_Text ContactMail;
	public TMP_Text ContactAddress;
	public TMP_Text ContactWebsite;


	[Header("Popup")]
	public GameObject Popup;

	//other variables
	private string temp;
	private bool show = false;
 
	// Register cloud reco callbacks
	void Awake()
	{
		mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
		mCloudRecoBehaviour.RegisterOnInitializedEventHandler(OnInitialized);
		mCloudRecoBehaviour.RegisterOnInitErrorEventHandler(OnInitError);
		mCloudRecoBehaviour.RegisterOnUpdateErrorEventHandler(OnUpdateError);
		mCloudRecoBehaviour.RegisterOnStateChangedEventHandler(OnStateChanged);
		mCloudRecoBehaviour.RegisterOnNewSearchResultEventHandler(OnNewSearchResult);


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
	//Unregister cloud reco callbacks when the handler is destroyed 
	void OnDestroy()
	{
		mCloudRecoBehaviour.UnregisterOnInitializedEventHandler(OnInitialized);
		mCloudRecoBehaviour.UnregisterOnInitErrorEventHandler(OnInitError);
		mCloudRecoBehaviour.UnregisterOnUpdateErrorEventHandler(OnUpdateError);
		mCloudRecoBehaviour.UnregisterOnStateChangedEventHandler(OnStateChanged);
		mCloudRecoBehaviour.UnregisterOnNewSearchResultEventHandler(OnNewSearchResult);
	}

	public void OnInitialized(TargetFinder targetFinder)
	{
		Debug.Log("Cloud Reco initialized");
	}
	public void OnInitError(TargetFinder.InitState initError)
	{
		Debug.Log("Cloud Reco init error " + initError.ToString());
	}
	public void OnUpdateError(TargetFinder.UpdateState updateError)
	{
		Debug.Log("Cloud Reco update error " + updateError.ToString());
	}

	public void OnStateChanged(bool scanning)
	{
		mIsScanning = scanning;
		if (scanning)
		{
			// clear all known trackables
			var tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
			tracker.GetTargetFinder<ImageTargetFinder>().ClearTrackables(false);
		}
	}

	// Here we handle a cloud target recognition event
	public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
	{

		TargetFinder.CloudRecoSearchResult cloudRecoSearchResult =
			(TargetFinder.CloudRecoSearchResult)targetSearchResult;
		// do something with the target metadata
		mTargetMetadata = cloudRecoSearchResult.MetaData;
		Debug.Log("Found image target named as ================>" + mTargetMetadata);

		// Build augmentation based on target 
		if (ImageTargetTemplate)
		{
			// enable the new result with the same ImageTargetBehaviour: 
			ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
			tracker.GetTargetFinder<ImageTargetFinder>().EnableTracking(targetSearchResult, ImageTargetTemplate.gameObject);
		}

		//CardCanvas.SetActive(true);

		StartCoroutine(ShowData(mTargetMetadata, emailLoginField.text));

		// stop the target finder (i.e. stop scanning the cloud)
		mCloudRecoBehaviour.CloudRecoEnabled = false;

	}

	private void InitializeFirebase()
	{
		Debug.Log("Setting up Firebase Auth");
		//Set the authentication instance object
		auth = FirebaseAuth.DefaultInstance;
		DBReference = FirebaseDatabase.DefaultInstance.RootReference;
	}

	public void LoginButton()
	{
		//Call the login coroutine passing the email and password
		StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
	}


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
			Debug.LogFormat("User signed in successfully: {0} ", User.Email);
			warningLoginText.text = "";
			confirmLoginText.text = "You can now scan the Card!";

			yield return new WaitForSeconds(1);
			confirmLoginText.text = "";
			LoginPage.SetActive(false);
			//ScanPage.SetActive(true);
		}
	}


	public string getNodeForDB(string copyEmailId)
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


	public IEnumerator ShowData(string key, string myEmail)
	{
		Debug.Log("Checking Permissions.");
		string dbkey = getNodeForDB(key);
		string mykey = getNodeForDB(myEmail);

		var DBTask = DBReference.Child("users").Child(dbkey).GetValueAsync();

		yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

		if (DBTask.Exception != null)
		{
			Debug.LogWarning(message: $"Failed to update task with {DBTask.Exception}");
		}
		else if (DBTask.Result.Value == null)
		{
			//No data exists yet
			//warningUpdateText.text = "Create your card to update details";
			//confirmUpdateText.text = "";
		}
		else
		{
			DataSnapshot snapshot = DBTask.Result;

			if (mykey == dbkey)
			{
				Debug.Log("This is my own card");
				show = true;
				StartCoroutine(SetCardValues(key));
			}

			else if ((Boolean)snapshot.Child("public").Value)
			{
				Debug.Log("Card is public");
				show = true;
				StartCoroutine(SetCardValues(key));
			}
			else
			{
				Debug.Log("Card is private, checking if you can access it!!");
				string mails = snapshot.Child("accessto").Value.ToString();

				string[] mailList = mails.Split(new char[] { ',' });


				foreach (string mail in mailList)
				{
					if (mail == mykey)
					{
						Debug.Log("Good news!! You can access this card!!");
						show = true;        //give permission to show card
						StartCoroutine(SetCardValues(key));
						break;
					}
				}

				yield return new WaitForSeconds(1); 

				Debug.Log("Bad news!! You can't access this card!!");

			}
		}		
	}

	public IEnumerator SetCardValues(string key)
    {
		Debug.Log("=============>Setting card values");

		string dbkey = getNodeForDB(key);
		var DBTask = DBReference.Child("users").Child(dbkey).GetValueAsync();

		yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

		if (DBTask.Exception != null)
		{
			Debug.LogWarning(message: $"Failed to update task with {DBTask.Exception}");
		}
		else if (DBTask.Result.Value == null)
		{
			//No data exists yet
			//warningUpdateText.text = "Create your card to update details";
			//confirmUpdateText.text = "";
		}
		else
		{
			//Data has been retrieved
			DataSnapshot snapshot = DBTask.Result;

			TitleName.text = snapshot.Child("profilename").Value.ToString();
			TitleProfession.text = snapshot.Child("profileprofession").Value.ToString();

			//Profile
			ProfileName.text = snapshot.Child("profilename").Value.ToString();
			ProfileAge.text = snapshot.Child("profileage").Value.ToString();
			ProfileProfession.text = snapshot.Child("profileprofession").Value.ToString();
			ProfileExperience.text = snapshot.Child("profileexperience").Value.ToString();

			//About
			temp = snapshot.Child("about").Value.ToString();
			if (temp != "")
				Aboutabout.text = temp;
			else
				Aboutabout.text = "Description not provided.";

			//Products
			//product1
			temp = snapshot.Child("productp1").Value.ToString();
			if (temp == "")
				Product1.text = "No name specified";
			else
				Product1.text = snapshot.Child("productp1").Value.ToString();

			//product2
			temp = snapshot.Child("productp2").Value.ToString();
			if (temp == "")
				Product2.text = "No name specified";  
			else
				Product2.text = snapshot.Child("productp2").Value.ToString();

			//product3
			temp = snapshot.Child("productp3").Value.ToString();
			if (temp == "")
				Product3.text = "No name specified";
			else
				Product3.text = snapshot.Child("productp3").Value.ToString(); 


			//Contact
			//phone
			temp = snapshot.Child("contactphone").Value.ToString();
			if (temp == "")
				ContactPhone.text = "Phone No. not provided";
			else
				ContactPhone.text = temp;

			//mail
			temp = snapshot.Child("contactmail").Value.ToString();
			if (temp == "")
				ContactMail.text = "Mail Id not provided";
			else
				ContactMail.text = temp;

			//address
			temp = snapshot.Child("contactaddress").Value.ToString();
			if (temp == "")
				ContactAddress.text = "Address not provided";
			else
				ContactAddress.text = temp;

			//website
			temp = snapshot.Child("contactwebsite").Value.ToString();
			if (temp == "")
				ContactWebsite.text = "Website not provided";
			else
				ContactWebsite.text = temp;

			temp = "";
			CardCanvas.SetActive(true);
		}
	}

	public void CallButton()
	{
		if (ContactPhone.text == "")
		{
			StartCoroutine(ShowPopup());
		}
		else
		{
			Application.OpenURL("tel:" + ContactPhone.text);
		}
	}

	public void MailButton()
    {
		Application.OpenURL("mailto:"+ContactMail.text);
    }

	public void WebsiteButton()
    {
		Application.OpenURL(ContactWebsite.text);
    }


	public IEnumerator ShowPopup()
    {
		Popup.SetActive(true);
		yield return new WaitForSeconds(2);
		Popup.SetActive(false);
	}

	public void CardLost()
    {
		Debug.Log("Card Lost:::::::::::::::::::::::::::::::::::::");
		CardCanvas.SetActive(false);
    }

	public void CardFound()
    {
		if (show)
		{
			Debug.Log("Card Found:::::::::::::::::::::::::::::::::::::");
			CardCanvas.SetActive(true);
		}
	}

	void OnGUI()
	{
		// Display current 'scanning' status
		//GUI.Box(new Rect(100, 100, 200, 50), mIsScanning ? "Scanning" : "Not scanning");
		// Display metadata of latest detected cloud-target
		//GUI.Box(new Rect(100, 200, 200, 50), "Metadata: " + mTargetMetadata);
		// If not scanning, show button
		// so that user can restart cloud scanning
		if (!mIsScanning)
		{
			if (GUI.Button(new Rect((Screen.width/2)-250 , Screen.height-100, 200, 70), "Restart Scanning"))
			{
				// Restart TargetFinder
				mCloudRecoBehaviour.CloudRecoEnabled = true;
				mTargetMetadata = "";
				CardLost();
				show = false;
			}
		}

		if (GUI.Button(new Rect(Screen.width-250, Screen.height-75, 200, 50), "Back"))
		{
			auth.SignOut();

			mCloudRecoBehaviour.CloudRecoEnabled = false;
			mTargetMetadata = "";
			CardLost();

			//Back to WelcomeUI
			SceneManager.LoadScene("SampleScene");
			SceneManager.UnloadSceneAsync("VuforiaScene");

		}
	}
}



