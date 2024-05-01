import {animate, animateChild, group, query, style, transition, trigger,} from '@angular/animations';

export const fadeInAnimation =
  trigger('routeAnimations', [
    transition('* <=> *', [
      query(':enter', [
        style({ opacity: 0 })
      ], {optional: true}),
      query(':leave', animateChild(), {optional: true}),
      group([
        query(':leave', [
          animate('100ms ease-out', style({ opacity: 0 }))
        ], {optional: true}),
        query(':enter', [
          animate('100ms ease-out', style({ opacity: 1 }))
        ], {optional: true})
      ]),
      query(':enter', animateChild(), {optional: true}),
    ]),
  ]);
