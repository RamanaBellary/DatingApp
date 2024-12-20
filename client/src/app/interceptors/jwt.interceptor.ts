import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../services/account.service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AccountService);

  if(accountService.currentUser()){
    console.log('User: '+JSON.stringify(accountService.currentUser()));
    req = req.clone({
      setHeaders:{
        Authorization: `Bearer ${accountService.currentUser()?.token}`
      }
    })
  }
  
  return next(req);
};
