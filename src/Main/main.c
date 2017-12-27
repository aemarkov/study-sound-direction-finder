#include "stm32f30x.h"
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

volatile uint16_t ADC_Result[8]={1,2,3,4,5,6,7,8};

uint16_t ADC1ConvertedValue;

void Adc1Init()
{
    NVIC_InitTypeDef NVIC_InitStructure; 
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
    dmaInit.DMA_BufferSize = 4;                                             // Размер буффера в памяти
    dmaInit.DMA_PeripheralInc = DMA_PeripheralInc_Disable;                  // Не инкрементировать адрес источника
    dmaInit.DMA_MemoryInc = DMA_MemoryInc_Enable;                           // Не инкрементировать адрес получателя
    dmaInit.DMA_PeripheralDataSize = DMA_PeripheralDataSize_Word;       // Размер данных - 2б
    dmaInit.DMA_MemoryDataSize = DMA_MemoryDataSize_Word;               // Размер данных - 2б
    dmaInit.DMA_Mode = DMA_Mode_Circular;                                   // 
    dmaInit.DMA_Priority = DMA_Priority_High;
    
    DMA_DeInit(DMA1_Channel1);
    DMA_Init(DMA1_Channel1, &dmaInit);
    DMA_Cmd(DMA1_Channel1, ENABLE);     
 
    // Настраиваем тактирование АЦП
    RCC_ADCCLKConfig(RCC_ADC12PLLCLK_Div2); 
 
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
    adcCommonInit.ADC_Mode = ADC_Mode_RegSimul;
    adcCommonInit.ADC_Clock = ADC_Clock_AsynClkMode;                    
    adcCommonInit.ADC_DMAAccessMode = ADC_DMAAccessMode_1;             
    adcCommonInit.ADC_DMAMode = ADC_DMAMode_Circular;                  
    adcCommonInit.ADC_TwoSamplingDelay = 0;          
    ADC_CommonInit(ADC1, &adcCommonInit);
  
 
    // Настройка АЦП
    ADC_StructInit(&adcInit);
    adcInit.ADC_ContinuousConvMode = ADC_ContinuousConvMode_Enable;
    adcInit.ADC_Resolution = ADC_Resolution_12b; 
    adcInit.ADC_ExternalTrigConvEvent = ADC_ExternalTrigConvEvent_0;         
    adcInit.ADC_ExternalTrigEventEdge = ADC_ExternalTrigEventEdge_None;
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
    RCC_AHBPeriphClockCmd(RCC_AHBPeriph_GPIOE, ENABLE);	
    GpioInitOutput(GPIOE, GPIO_Pin_9, GPIO_OType_PP, GPIO_PuPd_NOPULL, GPIO_Speed_Level_3);
    
    /*
    PC0 - ADC1 CH6 (alt) DMA CH1
    PC0 - ADC2 CH7 (alt) DMA CH2
    */
    Adc1Init();
      
    int wtf;
    while(1)
    {               
        wtf = ADC_Result[0];
        GPIOE->ODR ^= GPIO_Pin_9;
        //Delay_us(100000);
     }
}