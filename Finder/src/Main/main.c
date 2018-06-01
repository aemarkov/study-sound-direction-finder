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
uint8_t wtf[3] = {1, 2, 3};

int cnt = 0;
int sendEvery  = 3;

void HandleAdc(uint16_t* buffer, int length)
{    
        GPIOE->ODR^=GPIO_Pin_9;
        USB_Send_Data(header, HEADER_SIZE);
        USB_Send_Data((uint8_t*)buffer, length*sizeof(uint16_t));
}

void WtfTimerInit()
{
    RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM6, ENABLE);

    TIM_TimeBaseInitTypeDef timInit;    
    TIM_TimeBaseStructInit(&timInit);
    timInit.TIM_Prescaler = 72;
    timInit.TIM_Period = 1000;
    TIM_TimeBaseInit(TIM6, &timInit);
    
    TIM_ITConfig(TIM6, TIM_IT_Update, ENABLE);
    NVIC_EnableIRQ(TIM6_DAC1_IRQn);
    
    TIM_Cmd(TIM6, ENABLE);
}

void TIM6_DAC1_IRQHandler()
{
    if(TIM_GetITStatus(TIM6, TIM_IT_Update)!=RESET)
	{
        TIM_ClearITPendingBit(TIM6, TIM_IT_Update);
        GPIOE->ODR^=GPIO_Pin_9;
    }
}


int main(void)
{   
    /*Set_System();
    Set_USBClock();
    USB_Interrupts_Config();
    USB_Init();*/
    
    /*AdcInit(HandleAdc);
    DelayInit();*/
    
    RCC_AHBPeriphClockCmd(RCC_AHBPeriph_GPIOE, ENABLE);	    
    GpioInitOutput(GPIOE, GPIO_Pin_9 | GPIO_Pin_8, GPIO_OType_PP, GPIO_PuPd_NOPULL, GPIO_Speed_Level_3); 
        
    RCC_ClocksTypeDef clock;
    RCC_GetClocksFreq(&clock);
    
    GPIO_WriteBit(GPIOE, GPIO_Pin_8, 1); 
    WtfTimerInit();
    
    while(1);

    /*while(1)
    {
        GPIOE->ODR^=GPIO_Pin_9;
        for(int i = 0; i<1000000; i++);
    }*/
}

