import { Component, inject, input, Pipe, ViewChild } from '@angular/core';
import { MessageService } from '../../services/message.service';
import { TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
// import { CaseInsensitivePipe } from '../../customPipes/case-insensitive.pipe';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeagoModule, FormsModule, CommonModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css',
  // providers: [CaseInsensitivePipe]
})
export class MemberMessagesComponent {
  @ViewChild('messageForm') messageForm?: NgForm;
  messageService = inject(MessageService);
  username = input.required<string>();
  messageContent = '';
  // caseInsensitivePipe?: CaseInsensitivePipe;

  // constructor(casePipe: CaseInsensitivePipe){
  //   this.caseInsensitivePipe = casePipe
  // }

  sendMessage(){
    this.messageService.sendMessage(this.username(), this.messageContent).then(()=>{
      this.messageForm?.reset();
    })
  }
}
