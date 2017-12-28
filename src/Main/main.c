#include <stm32f30x.h>
#include <stm32f30x_rcc.h>
#include <stm32f30x_gpio.h>
#include <stm32f30x_adc.h>
#include <stm32f30x_dma.h>
#include "stm32f30x_misc.h"

#include <stdint.h>
#include <stdbool.h>

#include "config.h"
#include <Drivers/gpio/gpio.h>
#include <Drivers/gpio/discovery_leds.h>
#include <Drivers/delay/delay.h>
#include <Drivers/uart/uart.h>

#include "hw_config.h"
#include "usb_lib.h"
#include "usb_desc.h"
#include "usb_pwr.h"

#include <stdio.h>

#define ADC_BUFFER_SIZE 1024

volatile uint16_t ADC_Result[ADC_BUFFER_SIZE * 2];

uint16_t ADC1ConvertedValue;

// Настройка таймера для триггерения АЦП
// Получить ровно 4кГц частоту сэмплирования без таймера не 
// получится
void TimerInit()
{
    RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM6, ENABLE);
    
    // Частота - 4кГц
    TIM_TimeBaseInitTypeDef timInit;    
    TIM_TimeBaseStructInit(&timInit);
    timInit.TIM_Prescaler = 18;
    timInit.TIM_Period = 1000;
    TIM_TimeBaseInit(TIM6, &timInit);
    
    //TIM_ITConfig(TIM6, TIM_IT_Update, ENABLE);
    NVIC_EnableIRQ(TIM6_DAC1_IRQn);
    
    // Выходной триггер
    TIM_SelectOutputTrigger(TIM6, TIM_TRGOSource_Update);
    
    TIM_Cmd(TIM6, ENABLE);
}

void Adc1Init()
{
    ADC_InitTypeDef adcInit;
    DMA_InitTypeDef dmaInit;
    ADC_CommonInitTypeDef adcCommonInit;
    

   // Включаем тактирование DMA1, ADC12 и GPIOA
    RCC_AHBPeriphClockCmd(RCC_AHBPeriph_DMA1, ENABLE);
    RCC_AHBPeriphClockCmd(RCC_AHBPeriph_ADC12, ENABLE);
    RCC_AHBPeriphClockCmd(RCC_AHBPeriph_GPIOC, ENABLE);	
 
    // Настраиваем пин на работу в режиме аналогового входа
    GpioInitAnalog(GPIOC, GPIO_Pin_0 | GPIO_Pin_1);
 
    // Настройки DMA  
    dmaInit.DMA_PeripheralBaseAddr = (uint32_t)&ADC1_2->CDR;                // Адрес источника - регистр АЦП
    dmaInit.DMA_MemoryBaseAddr = (uint32_t)&ADC_Result;                     // Адрес получателя - буфер
    dmaInit.DMA_DIR = DMA_DIR_PeripheralSRC;                                // Из переферии в память
    dmaInit.DMA_M2M = DMA_M2M_Disable;                                      
    dmaInit.DMA_BufferSize = ADC_BUFFER_SIZE;                               // Размер буффера в памяти
    dmaInit.DMA_PeripheralInc = DMA_PeripheralInc_Disable;                  // Не инкрементировать адрес источника
    dmaInit.DMA_MemoryInc = DMA_MemoryInc_Enable;                           // Не инкрементировать адрес получателя
    dmaInit.DMA_PeripheralDataSize = DMA_PeripheralDataSize_Word;           // Размер данных - 4б
    dmaInit.DMA_MemoryDataSize = DMA_MemoryDataSize_Word;                   // Размер данных - 4б
    dmaInit.DMA_Mode = DMA_Mode_Circular;                                   // 
    dmaInit.DMA_Priority = DMA_Priority_High;
    
    DMA_DeInit(DMA1_Channel1);
    DMA_Init(DMA1_Channel1, &dmaInit);
    DMA_Cmd(DMA1_Channel1, ENABLE);     
 
    // Настраиваем тактирование АЦП
    RCC_ADCCLKConfig(RCC_ADC12PLLCLK_Div1); 
 
    // Калибровка АЦП
    ADC_VoltageRegulatorCmd(ADC1, ENABLE);
    ADC_SelectCalibrationMode(ADC1, ADC_CalibrationMode_Single);
    ADC_StartCalibration(ADC1);
    while(ADC_GetCalibrationStatus(ADC1) != RESET );
    
    ADC_VoltageRegulatorCmd(ADC2, ENABLE);
    ADC_SelectCalibrationMode(ADC2, ADC_CalibrationMode_Single);
    ADC_StartCalibration(ADC2);
    while(ADC_GetCalibrationStatus(ADC2) != RESET );
 
    // Настройка взаимодействия АЦП1 и АЦП2
    adcCommonInit.ADC_Mode = ADC_Mode_RegSimul;                             // Одновременный режим АЦП1 и АЦП2
    adcCommonInit.ADC_Clock = ADC_Clock_AsynClkMode;                    
    adcCommonInit.ADC_DMAAccessMode = ADC_DMAAccessMode_1;                  // MDMA для того, чтобы одновременно писать с обоих АЦП
    adcCommonInit.ADC_DMAMode = ADC_DMAMode_Circular;                       // Кольцевой ДМА
    adcCommonInit.ADC_TwoSamplingDelay = 0;          
    ADC_CommonInit(ADC1, &adcCommonInit);
  
 
    // Настройка АЦП
    ADC_StructInit(&adcInit);
    adcInit.ADC_ContinuousConvMode = ADC_ContinuousConvMode_Disable;
    adcInit.ADC_Resolution = ADC_Resolution_12b; 
    adcInit.ADC_ExternalTrigConvEvent = ADC_ExternalTrigConvEvent_13;           // Преобразование по TIM6 TRGO 
    adcInit.ADC_ExternalTrigEventEdge = ADC_ExternalTrigEventEdge_RisingEdge;  
    adcInit.ADC_DataAlign = ADC_DataAlign_Right;
    adcInit.ADC_OverrunMode = ADC_OverrunMode_Disable;   
    adcInit.ADC_AutoInjMode = ADC_AutoInjec_Disable;  
    adcInit.ADC_NbrOfRegChannel = 1;
    ADC_Init(ADC1, &adcInit);
 
    // Включение каналов АЦП
    ADC_RegularChannelConfig(ADC1, 6, 1, ADC_SampleTime_7Cycles5);
    ADC_RegularChannelConfig(ADC2, 7, 1, ADC_SampleTime_7Cycles5);
 
    // Включение ДМА для АЦП
    ADC_DMACmd(ADC1, ENABLE);
    ADC_DMAConfig(ADC1, ADC_DMAMode_Circular);
 
    // Наконец-то включаем АЦП
    ADC_Cmd(ADC1, ENABLE); 
    ADC_Cmd(ADC2, ENABLE);
 
 
    while(!ADC_GetFlagStatus(ADC1, ADC_FLAG_RDY));   
    ADC_StartConversion(ADC1);
}

int main(void)
{       
    DelayInit();
    
    Set_System();
    Set_USBClock();
    USB_Interrupts_Config();
    USB_Init();
    
    Adc1Init();
    TimerInit();
    
    RCC_AHBPeriphClockCmd(RCC_AHBPeriph_GPIOE, ENABLE);	    
    GpioInitOutput(GPIOE, GPIO_Pin_9 | GPIO_Pin_8, GPIO_OType_PP, GPIO_PuPd_NOPULL, GPIO_Speed_Level_3);   
    
    RCC_ClocksTypeDef clock;
    RCC_GetClocksFreq(&clock);

    while(1)
    {
        printf("%d %d\n", ADC_Result[0], ADC_Result[1]);
        for(int i = 0; i<100000; i++);
    }
}

/*void TIM6_DAC1_IRQHandler()
{
    if(TIM_GetITStatus(TIM6, TIM_IT_Update)!=RESET)
	{
        TIM_ClearITPendingBit(TIM6, TIM_IT_Update);
        GPIOE->ODR ^= GPIO_Pin_9;
    }
}*/