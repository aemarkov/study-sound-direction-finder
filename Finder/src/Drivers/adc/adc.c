#include "adc.h"

AdcHandler _handler;
uint16_t _adcBuffer[ADC_BUFFER_SIZE * 2];
void TimerInit();

/*
Частота сэмплирования: 4кГц
Размер буффера: 1024
Длительность: 256 мС
*/


void AdcInit(AdcHandler handler)
{
    _handler = handler;
    
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
    dmaInit.DMA_MemoryBaseAddr = (uint32_t)&_adcBuffer;                     // Адрес получателя - буфер
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
    
    // Прерывания DMA
    DMA_ITConfig(DMA1_Channel1, DMA_IT_TC, ENABLE);
    DMA_ITConfig(DMA1_Channel1, DMA_IT_HT, ENABLE);
    NVIC_EnableIRQ(DMA1_Channel1_IRQn);
 
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
 
    TimerInit();
 
    // Наконец-то включаем АЦП
    ADC_Cmd(ADC1, ENABLE); 
    ADC_Cmd(ADC2, ENABLE);    
 
    while(!ADC_GetFlagStatus(ADC1, ADC_FLAG_RDY));   
    ADC_StartConversion(ADC1);
}


// Настройка таймера для триггерения АЦП
// Получить ровно 4кГц частоту сэмплирования без таймера не 
// получится
void TimerInit()
{
    RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM6, ENABLE);
    
    // Частота - 4кГц
    TIM_TimeBaseInitTypeDef timInit;    
    TIM_TimeBaseStructInit(&timInit);
    timInit.TIM_Prescaler = 10;
    timInit.TIM_Period = 180;
    TIM_TimeBaseInit(TIM6, &timInit);
    
    //TIM_ITConfig(TIM6, TIM_IT_Update, ENABLE);
    //NVIC_EnableIRQ(TIM6_DAC1_IRQn);
    
    // Выходной триггер
    TIM_SelectOutputTrigger(TIM6, TIM_TRGOSource_Update);
    
    TIM_Cmd(TIM6, ENABLE);
}


void DMA1_Channel1_IRQHandler(void)
{
    if(DMA_GetITStatus(DMA1_IT_HT1)!=RESET)
    {
        //Half-transfer
        DMA_ClearITPendingBit(DMA1_IT_HT1);
        _handler(_adcBuffer, ADC_BUFFER_SIZE);
    }
    if(DMA_GetITStatus(DMA1_IT_TC1)!=RESET)
    {
        //transfer
        DMA_ClearITPendingBit(DMA1_IT_TC1);
        _handler(_adcBuffer+ADC_BUFFER_SIZE, ADC_BUFFER_SIZE);
    }
    
    DMA_ClearITPendingBit(DMA1_IT_GL1);
}
