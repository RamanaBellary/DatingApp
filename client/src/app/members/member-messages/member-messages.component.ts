import { AfterViewChecked, Component, inject, input, Pipe, viewChild, ViewChild } from '@angular/core';
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
export class MemberMessagesComponent implements AfterViewChecked {
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild('scrollMe') scrollContainer: any;
  messageService = inject(MessageService);
  username = input.required<string>();
  messageContent = '';
  loading = false;
  // caseInsensitivePipe?: CaseInsensitivePipe;

  // constructor(casePipe: CaseInsensitivePipe){
  //   this.caseInsensitivePipe = casePipe
  // }

  sendMessage(){
    this.loading = true;
    this.messageService.sendMessage(this.username(), this.messageContent).then(()=>{
      this.messageForm?.reset();
      this.scrollToBottom();
    }).finally(()=>this.loading = false)
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  private scrollToBottom(){
    if(this.scrollContainer){
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    }
  }
}
