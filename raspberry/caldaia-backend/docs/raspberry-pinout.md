|                                |                  |    |    |                    |                                     |
|--------------------------------|------------------|----|----|--------------------|-------------------------------------|
|                       adc VCC  | 3.3V power       |  1 |  2 | 5V power           |                                     |
|                       adc SDA  | GPIO 2 (SDA)     |  3 |  4 | 5V power           | VCC relay board                     |
|                       adc SCL  | GPIO 3 (SCL)     |  5 |  6 | Ground             | GND relay board                     |
|                                | GPIO 4 (GPCLK0)  |  7 |  8 | GPIO 14 (TXD)      |                                     |
|                       adc GND  | Ground           |  9 | 10 | GPIO 15 (RXD)      | TXD rotex                           |
|  POMPA RISCALD relay board IN4 | GPIO 17          | 11 | 12 | GPIO 18 (PCM_CLK)  |                                     |
|  BYPASS T.AMB  relay board IN3 | GPIO 27          | 13 | 14 | Ground             | GND txd rotex                       |
|  POMPA CAMINO  relay board IN2 | GPIO 22          | 15 | 16 | GPIO 23            | IN1 relay board ACCENSIONE CALDAIA  |
|                    ntc vulcano | 3.3V power       | 17 | 18 | GPIO 24            |                                     |
|                                | GPIO 10 (MOSI)   | 19 | 20 | Ground             |                                     |
|                                | GPIO  9 (MISO)   | 21 | 22 | GPIO 25            |                                     |
|                                | GPIO 11 (SCLK)   | 23 | 24 | GPIO  8 (CE0)      |                                     |
|                       adc ADDR | Ground           | 25 | 26 | GPIO  7 (CE1)      |                                     |
|                                | GPIO  0 (ID_SD)  | 27 | 28 | GPIO  1 (ID_SC)    |                                     |
|            termostato ambienti | GPIO  5          | 29 | 30 | Ground             |                                     |
|            termostato rotex    | GPIO  6          | 31 | 32 | GPIO 12 (PWM0)     |                                     |
|                                | GPIO 13 (PWM1)   | 33 | 34 | Ground             |                                     |
|                                | GPIO 19 (PCM_FS) | 35 | 36 | GPIO 16            |                                     |
|                                | GPIO 26          | 37 | 38 | GPIO 20 (PCM_DIN)  |                                     |
|                     ntc ground | Ground           | 39 | 40 | GPIO 21 (PCM_DOUT) |                                     |