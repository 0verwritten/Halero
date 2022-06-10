import React from 'react';
import ReactDOM from 'react-dom/client';
import reportWebVitals from './reportWebVitals';
import { Pager } from './page-manager';
import * as serviceWorkerRegistration from './serviceWorkerRegistration';
import { verifyToken } from './authentication/tokenVerification';

// verifyToken();
// setInterval( () => verifyToken(), 60 * 60 * 1000);
const root = ReactDOM.createRoot(document.getElementById('root')!);
root.render(
  <React.StrictMode>
    <Pager />
  </React.StrictMode>
);

serviceWorkerRegistration.unregister();

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
// reportWebVitals(console.log);
