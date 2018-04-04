#include <stm32f30x.h>
#include <stm32f30x_rcc.h>
#include <stm32f30x_gpio.h>
#include <stm32f30x_adc.h>
#include <stm32f30x_dma.h>
#include "stm32f30x_misc.h"

#include <stdint.h>
#include <stdbool.h>

#include <Drivers/gpio/gpio.h>
#include <Drivers/delay/delay.h>
#include <Drivers/adc/adc.h>

#include "hw_config.h"
#include "usb_lib.h"
#include "usb_desc.h"
#include "usb_pwr.h"

#include <stdio.h>

#define HEADER_SIZE 3
uint8_t header[HEADER_SIZE] = {0x32, 0xFA, 0x12};

int cnt = 0;
int sendEvery  = 3;

void HandleAdc(uint16_t* buffer, int length)
{
    cnt++;
    if(cnt % sendEvery == 0)
    {    
        GPIOE->ODR^=GPIO_Pin_8;
        USB_Send_Data(header, HEADER_SIZE);       
        USB_Send_Data((uint8_t*)buffer, length*sizeof(uint16_t));
        //USB_SetBuffer((uint8_t*)buffer, length * sizeof(uint16_t));
    }
}

int main(void)
{   
    Set_System();
    Set_USBClock();
    USB_Interrupts_Config();
    USB_Init();
    
    AdcInit(HandleAdc);
    
    RCC_AHBPeriphClockCmd(RCC_AHBPeriph_GPIOE, ENABLE);	    
    GpioInitOutput(GPIOE, GPIO_Pin_9 | GPIO_Pin_8, GPIO_OType_PP, GPIO_PuPd_NOPULL, GPIO_Speed_Level_3); 
    
    RCC_ClocksTypeDef clock;
    RCC_GetClocksFreq(&clock);

    while(1)
    {
        /*for(int i = 0; i<100000; i++);
        GPIOE->ODR^=GPIO_Pin_8;*/
    }
}

