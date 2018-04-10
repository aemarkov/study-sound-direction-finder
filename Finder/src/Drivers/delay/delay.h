/*
Реализация задержек при помощи DWT
(Data Watchpoint and Trace Unit)

На основе https://hubstub.ru/stm32/101-funkciya-zaderzhki-stm32.html

Обладает высокой точностью задержек вплоть до нескольких микросекунд.
Примерная систематическая ошибка +0.5us
*/


#ifndef __DELAY_H__
#define __DELAY_H__

#include <stm32f30x.h>

/**
    \brief Необходимые настройки для работы
*/
void DelayInit(void);

/**
    \brief Задержка
    \param[in] us Количество микросекунд задержки
*/

void Delay_us(uint32_t us);

/**
    \brief Задержка
    \param[in] us Количество миллисекунд задержки
*/

void Delay_ms(uint32_t us);

void Timer_start(void);
float Timer_get(void);

#endif
