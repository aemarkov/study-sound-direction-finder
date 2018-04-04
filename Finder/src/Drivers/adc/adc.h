#ifndef __ADC_H__
#define __ADC_H__

#include <stdint.h>
#include <stm32f30x.h>
#include <stm32f30x_rcc.h>
#include <stm32f30x_gpio.h>
#include <stm32f30x_adc.h>
#include <stm32f30x_dma.h>
#include "stm32f30x_misc.h"
#include <Drivers/gpio/gpio.h>
#include <config.h>

typedef void (*AdcHandler)(uint16_t* buffer, int length);
void AdcInit(AdcHandler handler);

#endif
