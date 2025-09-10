import * as msal from '@azure/msal-browser';

const isBrowser = typeof window !== 'undefined';

export const msalInstance = isBrowser
  ? new msal.PublicClientApplication({
      auth: {
        clientId: '91badb0a-8a77-4b2d-865c-143f5362eb4b', // Microsoft Client ID
        authority: 'https://login.microsoftonline.com/f948c942-70c1-4359-b895-f4266f875723', // Tenant ID
        redirectUri: window.location.origin
      }
    })
  : null;

export const initializeMsal = async () => {
  if (isBrowser && msalInstance) {
    await msalInstance.initialize();
  }
};

export const isBrowserEnv = isBrowser;
