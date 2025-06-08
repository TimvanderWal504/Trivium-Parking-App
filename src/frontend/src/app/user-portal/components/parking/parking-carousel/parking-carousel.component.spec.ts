import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ParkingCarouselComponent } from './parking-carousel.component';

describe('ParkingCarouselComponent', () => {
  let component: ParkingCarouselComponent;
  let fixture: ComponentFixture<ParkingCarouselComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ParkingCarouselComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ParkingCarouselComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
