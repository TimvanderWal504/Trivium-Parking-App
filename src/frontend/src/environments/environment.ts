// This file can be replaced during build by using the `fileReplacements` array in `angular.json`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  // TODO: Replace with your **DEVELOPMENT** Firebase project configuration object
  firebase: {
    apiKey: 'YOUR_API_KEY',
    authDomain: 'YOUR_AUTH_DOMAIN',
    projectId: 'YOUR_PROJECT_ID',
    storageBucket: 'YOUR_STORAGE-BUCKET',
    messagingSenderId: 'YOUR_MESSAGINGSENDER_ID',
    appId: 'YOUR_APP_ID',
    // measurementId: "YOUR_DEV_MEASUREMENT_ID" // Optional: Add if using Analytics
  },
  backendApiUrl: 'YOUR_BACKENDAPI_URL', // Default backend URL for base environment
};
