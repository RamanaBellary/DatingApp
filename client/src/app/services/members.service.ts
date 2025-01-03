import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { AccountService } from './account.service';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;
  user = this.accountService.currentUser();
  members = signal<Member[]>([]);
  
  getMembers(){
    return this.http.get<Member[]>(this.baseUrl + 'users').subscribe({
      next: members => this.members.set(members)
    })
  }

  getMember(username: string){
    const member = this.members().find(x=>x.UserName === username);

    if(member != undefined) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member){
    return this.http.put(this.baseUrl + 'users', member).pipe(
      tap(() => {
        this.members.update(members => members.map(m=> m.UserName === member.UserName ? member : m))
      })
    );
  }

  setMainPhoto(photo: Photo){
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.Id, {}).pipe(
      tap(()=>{
        this.members.update(members => members.map(m => {
          if(m.Photos.includes(photo)){
            m.PhotoUrl = photo.URL
          }
          return m;
        }))
      })
    );
  }

  deletePhoto(photo: Photo){
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.Id).pipe(
      tap(()=>{
        this.members.update(members => members.map(m=>{
          if(m.Photos.includes(photo)){
            m.Photos = m.Photos.filter(p=>p.Id !== photo.Id)
          }
          return m;
        }))
      })
    );
  }
}
