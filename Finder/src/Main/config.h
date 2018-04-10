#ifndef __CONFIG_H__
#define __CONFIG_H__


/*
SYSCLOCK = 72.000.000

window    10 мс
cnt       1000 отсчетов

f = cnt * 1000 / window
f = 1000 * 100 / 10 = 10.000
*/

/*
Количество измерений ПО ОДНОМУ КАНАЛУ
за одну передачу
*/
#define ADC_WINDOW_SIZE 500
#define ADC_TIMER_PRESCALER 72
#define ADC_TIMER_PERIOD 20

#endif
