import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BizumCreateComponent } from './bizum-create.component';

describe('BizumCreateComponent', () => {
  let component: BizumCreateComponent;
  let fixture: ComponentFixture<BizumCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BizumCreateComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(BizumCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
