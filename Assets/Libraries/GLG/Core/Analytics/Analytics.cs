﻿using System.Collections.Generic;
using UnityEngine;

namespace GLG.Analytics
{
    public enum AnalyticsProvider { Facebook, AppMetrica }
    public class AnalyticsMessage
    {
        public string id;
        public Dictionary<string, object> data;
    }
    public class AnalyticsWrapper : MonoBehaviour, IManaged
    {
        [SerializeField] private bool _debug = false;
        [SerializeField] private float _sendFromQueueDelay;

        private List<IAnalyticsProvider> _providers = new List<IAnalyticsProvider>();
        private float _nextTimeToSend = 0;

        #region STANDARD MESSAGES
        private void Awake()
        {
            Kernel.RegisterManaged(this);
            Initialize();
        }
        private void OnDestroy()
        {
            Kernel.UnregisterManaged(this);
        }
        public void ManagedUpdate()
        {
            if (Time.time > _nextTimeToSend)
            {
                foreach (var item in _providers)
                {
                    item.SendMessagesInQueue();
                }
            }
        }
        #endregion

        #region PUBLIC METHODS
        public void Flush()
        {
            foreach (var item in _providers)
            {
                item.SendMessagesInQueue();
                item.Flush();
            }
        }
        public int Track(string id)
        {
            int providerErrors = 0;
            foreach (var item in _providers)
            {
                if (!item.Track(id))
                {
                    providerErrors++;
                    item.EnqueueMessage(new AnalyticsMessage { id = id });
                    if (_debug)
                    {
                        Debug.Log($"[Analytics] EnqueueMessage:{item.AnalyticsProviderName}   id: {id}");
                    }
                }
                else
                {
                    if (_debug)
                    {
                        Debug.Log($"[Analytics] Track:{item.AnalyticsProviderName}   id: {id}");
                    }
                }
            }
            return providerErrors;
        }
        public int Track(string id, string name, string value)
        {
            int providerErrors = 0;
            foreach (var item in _providers)
            {
                if (!item.Track(id, name, value))
                {
                    providerErrors++;
                    item.EnqueueMessage(new AnalyticsMessage { id = id, data = new Dictionary<string, object>() { { name, value } } });
                    if (_debug)
                    {
                        Debug.Log($"[Analytics] EnqueueMessage:{item.AnalyticsProviderName}   id: {id}");
                    }
                }
                else
                {
                    if (_debug)
                    {
                        Debug.Log($"[Analytics] Track:{item.AnalyticsProviderName}   id: {id}");
                    }
                }
            }
            return providerErrors;
        }
        public int Track(string id, params (string name, string value)[] values)
        {
            int providerErrors = 0;
            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (var item in values)
            {
                data.Add(item.name, item.value);
            }
            foreach (var item in _providers)
            {
                if (!item.Track(id, data))
                {
                    providerErrors++;
                    item.EnqueueMessage(new AnalyticsMessage { id = id, data = data });
                    if (_debug)
                    {
                        Debug.Log($"[Analytics] EnqueueMessage:{item.AnalyticsProviderName}   id: {id}");
                    }
                }
                else
                {
                    if (_debug)
                    {
                        Debug.Log($"[Analytics] Track:{item.AnalyticsProviderName}   id: {id}");
                    }
                }
            }
            return providerErrors;
        }
        public int Track(string id, Dictionary<string, object> data)
        {
            int providerErrors = 0;
            foreach (var item in _providers)
            {
                if (!item.Track(id, data))
                {
                    providerErrors++;
                    item.EnqueueMessage(new AnalyticsMessage { id = id, data = data });
                    if (_debug)
                    {
                        Debug.Log($"[Analytics] EnqueueMessage:{item.AnalyticsProviderName}   id: {id}");
                    }
                }
                else
                {
                    if (_debug)
                    {
                        Debug.Log($"[Analytics] Track:{item.AnalyticsProviderName}   id: {id}");
                    }
                }
            }
            return providerErrors;
        }
        #endregion


        #region PRIVATE METHODS
        private void Initialize()
        {
#if AppMetrica
        _providers.Add(new AppMetricaAnalyticsAdapter());
        _providersCount++;
#endif
#if Facebook
        _providers.Add(new FacebookAnalyticsAdapter());
        _providersCount++;
#endif
            foreach (var item in _providers)
            {
                item.Initialize();
            }
            if (_debug)
            {
                Debug.Log($"[Analytics] Initialized. Providers:{_providers.Count}");
            }
        }
        #endregion
    }
}