// import { HttpInterceptorFn } from '@angular/common/http';
// import * as msal from '@azure/msal-browser';

// export const msalInstance = new msal.PublicClientApplication({
//   auth: {
//     clientId: '1c2ae038-c326-497b-a1e8-17ab51dfb0cd',
//     authority: 'https://login.microsoftonline.com/5b62eddc-15a0-4ebe-b43a-e49fd05af99f',
//     redirectUri: 'http://localhost:4200'
//   }
// });

// export const authInterceptor: HttpInterceptorFn = (req, next) => {
//     const token = localStorage.getItem('jwt');
//     if (token) {
//         req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
//     }
//     return next(req);
// };

import { HttpInterceptorFn } from '@angular/common/http';
 
export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const token = localStorage.getItem('jwt') || localStorage.getItem('jwtToken');
    if (token) {
        req = req.clone({ 
            setHeaders: { 
                Authorization: `Bearer ${token}` 
            } 
        });
    }
    return next(req);
};