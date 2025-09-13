import { Component, afterNextRender } from '@angular/core';
import { LoggingService } from '../../../services/log.service';

@Component({
  selector: 'app-logo',
  standalone: true,
  templateUrl: './logo.component.html',
  styleUrls: ['./logo.component.scss']
})
export class LogoComponent {

  constructor(private log: LoggingService) {

    afterNextRender(() => {
      this.log.verbose('LogoComponent.afterNextRender()', this);

    });
  }
}
