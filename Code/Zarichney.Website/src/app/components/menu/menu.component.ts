import { Component, Directive, ElementRef, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
// Removed unused service imports

@Directive({
  selector: 'ul[menuDirective]',
  standalone: true
})
export class MenuDirective {
  constructor(public elementRef: ElementRef) { }
}

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    MenuDirective
  ],
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent {

  @ViewChild(MenuDirective)
  menuContainer!: MenuDirective;
  menuOverflow: boolean = false;
  showMobileMenu: boolean = false;

  constructor() {
    // Removed afterNextRender from constructor
  }

  ngAfterViewInit() {
  }

  handleResize(_resize: any) {
    // this.log.info('onResize', resize);
    // let height = this.menuContainer!.nativeElement.clientHeight;
    // let width = this.menuContainer!.nativeElement.clientWidth;
    // this.log.info(`Width: ${width}, Height: ${height}`, this.menuContainer!.nativeElement);
  }

  hasOverflowingChildren(element: HTMLElement): boolean {
    let totalWidth = 0;
    // for (let child of children) {
    //   totalWidth += (child as HTMLElement).offsetWidth;
    // }
    return totalWidth > element.offsetWidth;
  }

  toggleMenu() {
    this.showMobileMenu = !this.showMobileMenu;
  }

}
