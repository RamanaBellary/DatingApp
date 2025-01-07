import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../services/members.service';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../_models/member';
import {TabDirective, TabsModule} from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoModule, TimeagoPipe } from 'ngx-timeago';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit {
  private membService = inject(MembersService);
  private route = inject(ActivatedRoute);
  member?: Member = {} as Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  

  ngOnInit(): void {
    this.loadMember();
    this.member && this.member.Photos.map(p => {
      this.images.push(new ImageItem({src: p.URL, thumb: p.URL}))
    })
  }

  loadMember(){
    const username = this.route.snapshot.paramMap.get('username');
    if(!username) return;

    this.membService.getMember(username)
    .subscribe({
      next: member=> {
        this.member = member;
      }
    })
  }
}
