import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../services/account.service';
import { environment } from '../../../environments/environment';
import { MembersService } from '../../services/members.service';
import { Photo } from '../../_models/photo';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, NgStyle, FileUploadModule, DecimalPipe],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})
export class PhotoEditorComponent implements OnInit {
  private accountService = inject(AccountService);
  private memberService = inject(MembersService);
  member = input.required<Member>();
  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  memberChange = output<Member>();

  ngOnInit(): void {
    this.initializeUploader();    
  }

  fileOverBase(event: any){
    this.hasBaseDropZoneOver = event;
  }

  deletePhoto(photo: Photo){
    this.memberService.deletePhoto(photo).subscribe({
      next: _=>{
        const updateMember = {...this.member()};
        updateMember.Photos = updateMember.Photos.filter(p=>p.Id != photo.Id);
        this.memberChange.emit(updateMember);
      }
    })
  }

  setMainPhoto(photo: Photo){
    this.memberService.setMainPhoto(photo).subscribe({
      next: _=>{
        const user = this.accountService.currentUser();
        if(user){
          user.photourl = photo.URL;
          this.accountService.setCurrentUser(user);
        }

        const updatedMember = {...this.member()}
        updatedMember.PhotoUrl = photo.URL;
        updatedMember.Photos.forEach(p=>{
          if(p.IsMain) p.IsMain = false;
          if(p.Id == photo.Id) p.IsMain = true;
        });
        
        this.memberChange.emit(updatedMember);
      }
    })
  }

  initializeUploader(){
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer '+ this.accountService.currentUser()?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    console.log('uploader authoken: ' + this.uploader?.authToken);

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      console.log(response);
      const photo = JSON.parse(response);
      const updatedMember = {...this.member()};
      updatedMember.Photos.push(photo);
      this.memberChange.emit(updatedMember);

      if(photo.IsMain){
        const user = this.accountService.currentUser();
        if(user){
          user.photourl = photo.URL;
          this.accountService.setCurrentUser(user);
        }

        updatedMember.PhotoUrl = photo.URL;
        updatedMember.Photos.forEach(p=>{
          if(p.IsMain) p.IsMain = false;
          if(p.Id == photo.Id) p.IsMain = true;
        });
        
        this.memberChange.emit(updatedMember);
      }
    }
  }  
}
