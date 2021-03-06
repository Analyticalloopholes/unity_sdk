﻿using System;
using System.Collections.Generic;

using UnityEngine;

namespace com.adjust.sdk
{
    public class Adjust : MonoBehaviour
    {
        #region Adjust fields
        private const string errorMessage = "adjust: SDK not started. Start it manually using the 'start' method.";

        private static IAdjust instance = null;

        private static Action<AdjustEventSuccess> eventSuccessDelegate = null;
        private static Action<AdjustEventFailure> eventFailureDelegate = null;
        private static Action<AdjustSessionSuccess> sessionSuccessDelegate = null;
        private static Action<AdjustSessionFailure> sessionFailureDelegate = null;
        private static Action<AdjustAttribution> attributionChangedDelegate = null;

        public bool startManually = true;
        public bool eventBuffering = false;
        public bool printAttribution = true;

        public string appToken = "{Your App Token}";

        public AdjustLogLevel logLevel = AdjustLogLevel.Info;
        public AdjustEnvironment environment = AdjustEnvironment.Sandbox;
        #endregion

        #region Unity lifecycle methods
        void Awake ()
        {
            if (Adjust.instance != null)
            {
                  return;
              }
              
            DontDestroyOnLoad (transform.gameObject);

            if (!this.startManually)
            {
                AdjustConfig adjustConfig = new AdjustConfig (this.appToken, this.environment);
                adjustConfig.setLogLevel (this.logLevel);
                adjustConfig.setEventBufferingEnabled (eventBuffering);

                if (printAttribution)
                {
                    adjustConfig.setEventSuccessDelegate (EventSuccessCallback);
                    adjustConfig.setEventFailureDelegate (EventFailureCallback);
                    adjustConfig.setSessionSuccessDelegate (SessionSuccessCallback);
                    adjustConfig.setSessionFailureDelegate (SessionFailureCallback);
                    adjustConfig.setAttributionChangedDelegate (AttributionChangedCallback);
                }

                Adjust.start (adjustConfig);
            }
        }

        void OnApplicationPause (bool pauseStatus) 
        {
            if (Adjust.instance == null)
            {
                return;
            }
            
            if (pauseStatus)
            {
                Adjust.instance.onPause ();
            }
            else
            {
                Adjust.instance.onResume ();
            }
        }
        #endregion

        #region Adjust methods
        public static void start (AdjustConfig adjustConfig)
        {
            if (Adjust.instance != null)
            {
                Debug.Log ("adjust: Error, SDK already started.");
                return;
            }

            if (adjustConfig == null)
            {
                Debug.Log ("adjust: Missing config to start.");
                return;
            }

            #if UNITY_EDITOR
                Adjust.instance = null;
            #elif UNITY_IOS
                Adjust.instance = new AdjustiOS ();
            #elif UNITY_ANDROID
                Adjust.instance = new AdjustAndroid ();
            #elif UNITY_WP8
                Adjust.instance = new AdjustWP8 ();
            #elif UNITY_METRO
                Adjust.instance = new AdjustMetro ();
            #else
                Adjust.instance = null;
            #endif

            if (Adjust.instance == null)
            {
                Debug.Log ("adjust: SDK can only be used in Android, iOS, Windows Phone 8 or Windows Store apps.");
                return;
            }

            Adjust.eventSuccessDelegate = adjustConfig.getEventSuccessDelegate ();
            Adjust.eventFailureDelegate = adjustConfig.getEventFailureDelegate ();
            Adjust.sessionSuccessDelegate = adjustConfig.getSessionSuccessDelegate ();
            Adjust.sessionFailureDelegate = adjustConfig.getSessionFailureDelegate ();
            Adjust.attributionChangedDelegate = adjustConfig.getAttributionChangedDelegate ();

            Adjust.instance.start (adjustConfig);
        }

        public static void trackEvent (AdjustEvent adjustEvent)
        {
            if (Adjust.instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }
            
            if (adjustEvent == null)
            {
                Debug.Log ("adjust: Missing event to track.");
                return;
            }
            
            Adjust.instance.trackEvent (adjustEvent);
        }

        public static void setEnabled (bool enabled) 
        {
            if (Adjust.instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            Adjust.instance.setEnabled (enabled);
        }
        
        public static bool isEnabled () 
        {
            if (Adjust.instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return false;
            }

            return Adjust.instance.isEnabled ();
        }
        
        public static void setOfflineMode (bool enabled) 
        {
            if (Adjust.instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            Adjust.instance.setOfflineMode (enabled);
        }
        
        // iOS specific methods
        public static void setDeviceToken (string deviceToken)
        {
            if (Adjust.instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            Adjust.instance.setDeviceToken (deviceToken);
        }

        public static string getIdfa ()
        {
            if (Adjust.instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return null;
            }

            return Adjust.instance.getIdfa ();
        }

        // Android specific methods
        public static void setReferrer (string referrer)
        {
            if (Adjust.instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            Adjust.instance.setReferrer (referrer);
        }

        public static void getGoogleAdId (Action<string> onDeviceIdsRead)
        {
            if (Adjust.instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            Adjust.instance.getGoogleAdId (onDeviceIdsRead);
        }
        #endregion

        #region Attribution callback
        public void GetNativeAttribution (string attributionData)
        {
            if (instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            if (Adjust.attributionChangedDelegate == null)
            {
                Debug.Log ("adjust: Attribution changed delegate was not set.");
                return;
            }
            
            var attribution = new AdjustAttribution (attributionData);
            Adjust.attributionChangedDelegate (attribution);
        }

        public void GetNativeEventSuccess (string eventSuccessData)
        {
            if (instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            if (Adjust.eventSuccessDelegate == null)
            {
                Debug.Log ("adjust: Event success delegate was not set.");
                return;
            }

            var eventSuccess = new AdjustEventSuccess (eventSuccessData);
            Adjust.eventSuccessDelegate (eventSuccess);
        }

        public void GetNativeEventFailure (string eventFailureData)
        {
            if (instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            if (Adjust.eventFailureDelegate == null)
            {
                Debug.Log ("adjust: Event failure delegate was not set.");
                return;
            }

            var eventFailure = new AdjustEventFailure (eventFailureData);
            Adjust.eventFailureDelegate (eventFailure);
        }

        public void GetNativeSessionSuccess (string sessionSuccessData)
        {
            if (instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            if (Adjust.sessionSuccessDelegate == null)
            {
                Debug.Log ("adjust: Session success delegate was not set.");
                return;
            }

            var sessionSuccess = new AdjustSessionSuccess (sessionSuccessData);
            Adjust.sessionSuccessDelegate (sessionSuccess);
        }

        public void GetNativeSessionFailure (string sessionFailureData)
        {
            if (instance == null)
            {
                Debug.Log (Adjust.errorMessage);
                return;
            }

            if (Adjust.sessionFailureDelegate == null)
            {
                Debug.Log ("adjust: Session failure delegate was not set.");
                return;
            }

            var sessionFailure = new AdjustSessionFailure (sessionFailureData);
            Adjust.sessionFailureDelegate (sessionFailure);
        }

        #endregion

        #region Private & helper methods

        // Our delegate for detecting attribution changes if choosen not to start manually.
        private void AttributionChangedCallback (AdjustAttribution attributionData)
        {
            Debug.Log ("Attribution changed!");

            if (attributionData.trackerName != null)
            {
                Debug.Log ("trackerName " + attributionData.trackerName);
            }

            if (attributionData.trackerToken != null)
            {
                Debug.Log ("trackerToken " + attributionData.trackerToken);
            }

            if (attributionData.network != null)
            {
                Debug.Log ("network " + attributionData.network);
            }

            if (attributionData.campaign != null)
            {
                Debug.Log ("campaign " + attributionData.campaign);
            }

            if (attributionData.adgroup != null)
            {
                Debug.Log ("adgroup " + attributionData.adgroup);
            }

            if (attributionData.creative != null)
            {
                Debug.Log ("creative " + attributionData.creative);
            }

            if (attributionData.clickLabel != null)
            {
                Debug.Log ("clickLabel" + attributionData.clickLabel);
            }
        }

        // Our delegate for detecting successful event tracking if choosen not to start manually.
        private void EventSuccessCallback (AdjustEventSuccess eventSuccessData)
        {
            Debug.Log ("Event tracked successfully!");

            if (eventSuccessData.Message != null)
            {
                Debug.Log ("Message: " + eventSuccessData.Message);
            }

            if (eventSuccessData.Timestamp != null)
            {
                Debug.Log ("Timestamp: " + eventSuccessData.Timestamp);
            }

            if (eventSuccessData.Adid != null)
            {
                Debug.Log ("Adid: " + eventSuccessData.Adid);
            }

            if (eventSuccessData.EventToken != null)
            {
                Debug.Log ("EventToken: " + eventSuccessData.EventToken);
            }

            if (eventSuccessData.JsonResponse != null)
            {
                Debug.Log ("JsonResponse: " + eventSuccessData.GetJsonResponse ());
            }
        }

        // Our delegate for detecting failed event tracking if choosen not to start manually.
        private void EventFailureCallback (AdjustEventFailure eventFailureData)
        {
            Debug.Log ("Event tracking failed!");

            if (eventFailureData.Message != null)
            {
                Debug.Log ("Message: " + eventFailureData.Message);
            }

            if (eventFailureData.Timestamp != null)
            {
                Debug.Log ("Timestamp: " + eventFailureData.Timestamp);
            }

            if (eventFailureData.Adid != null)
            {
                Debug.Log ("Adid: " + eventFailureData.Adid);
            }

            if (eventFailureData.EventToken != null)
            {
                Debug.Log ("EventToken: " + eventFailureData.EventToken);
            }

            Debug.Log ("WillRetry: " + eventFailureData.WillRetry.ToString ());

            if (eventFailureData.JsonResponse != null)
            {
                Debug.Log ("JsonResponse: " + eventFailureData.GetJsonResponse ());
            }
        }

        // Our delegate for detecting successful session tracking if choosen not to start manually.
        private void SessionSuccessCallback (AdjustSessionSuccess sessionSuccessData)
        {
            Debug.Log ("Session tracked successfully!");

            if (sessionSuccessData.Message != null)
            {
                Debug.Log ("Message: " + sessionSuccessData.Message);
            }

            if (sessionSuccessData.Timestamp != null)
            {
                Debug.Log ("Timestamp: " + sessionSuccessData.Timestamp);
            }

            if (sessionSuccessData.Adid != null)
            {
                Debug.Log ("Adid: " + sessionSuccessData.Adid);
            }

            if (sessionSuccessData.JsonResponse != null)
            {
                Debug.Log ("JsonResponse: " + sessionSuccessData.GetJsonResponse ());
            }
        }

        // Our delegate for detecting failed session tracking if choosen not to start manually.
        private void SessionFailureCallback (AdjustSessionFailure sessionFailureData)
        {
            Debug.Log ("Session tracking failed!");

            if (sessionFailureData.Message != null)
            {
                Debug.Log ("Message: " + sessionFailureData.Message);
            }

            if (sessionFailureData.Timestamp != null)
            {
                Debug.Log ("Timestamp: " + sessionFailureData.Timestamp);
            }

            if (sessionFailureData.Adid != null)
            {
                Debug.Log ("Adid: " + sessionFailureData.Adid);
            }

            Debug.Log ("WillRetry: " + sessionFailureData.WillRetry.ToString ());

            if (sessionFailureData.JsonResponse != null)
            {
                Debug.Log ("JsonResponse: " + sessionFailureData.GetJsonResponse ());
            }
        }
        #endregion
    }
}
