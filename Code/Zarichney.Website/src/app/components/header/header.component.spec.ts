import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { provideMockStore } from '@ngrx/store/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AppHeaderComponent } from './header.component';

@Component({
  selector: 'app-logo',
  template: '<div>Logo</div>',
  standalone: true
})
class MockLogoComponent {}

@Component({
  selector: 'app-menu',
  template: '<div>Menu</div>',
  standalone: true
})
class MockMenuComponent {}

describe('AppHeaderComponent', () => {
  let component: AppHeaderComponent;
  let fixture: ComponentFixture<AppHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppHeaderComponent, MockLogoComponent, MockMenuComponent, RouterTestingModule],
      providers: [provideMockStore({})]
    }).compileComponents();

    fixture = TestBed.createComponent(AppHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render header container', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.header-container')).toBeTruthy();
  });
});