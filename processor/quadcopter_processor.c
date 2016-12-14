/*---------------------------------------------------------------------------*
Quadrotor Kontrol Uygulamasi
Yazan: Onur AKGÜN

25.06.2015
*----------------------------------------------------------------------------*/

#include <stdio.h>
#include <math.h>
#include "stm32f4xx.h"
#include "tm_stm32f4_usart.h"
#include "tm_stm32f4_delay.h"
#include "tm_stm32f4_pwm.h"
#include "tm_stm32f4_disco.h" 
#include "defines.h"
#include "stm32f4xx_rcc.h"
#include "stm32f4xx_tim.h"
#include "misc.h"
#include "tm_stm32f4_pwm.h"
#include "stm32f4_servo_onur.h"
#include "tm_stm32f4_adc.h"
#include "tm_stm32f4_mpu6050.h"
#include "tm_stm32f4_watchdog.h"
#include "stm32f4_kameraServo.h"

#define Veri_Adedi 10
#define Yuvarlanma_Ondeger -1.0
#define Yunuslama_Ondeger -1.0
#define Yonelme_Ondeger 54
#define IntegralToplam 200
#define HareketAcisi 10

#define PT_HAS_DATA        0x80  //10000000
#define PT_IS_BATCH        0x40  //01000000
#define PT_BATCH_LEN       0x08  //Batch uzunlugu = 1 

TM_SERVO_t Motor1, Motor2, Motor3, Motor4;
TM_SERVOKamera_t Motor_KameraSS, Motor_KameraYA;

float veriDizi_integralRoll[10],veriDizi_integralPitch[10],veriDizi_integralYaw[10];
uint8_t sayac_integral = 0,sayac_turev;
double motor1_Katsayi1=1,motor2_Katsayi1=1,motor3_Katsayi1=1,motor4_Katsayi1=1,motor1_Katsayi2=1,motor2_Katsayi2=1,motor3_Katsayi2=1,motor4_Katsayi2=1;
double Yuvarlanma=0,Yunuslama=0,Yonelme=0;
double refYuvarlanma=0, refYunuslama=0, refYonelme=0;
double hataX=0, hataY=0, hataZ=0;
double turevX=0, integralX=0, turevY=0, integralY=0, turevZ, integralZ;
uint8_t gelenVeri[4] = {0,0,0,0},sayac=0,k=0,i=0;
uint16_t denetim=0, veri=0;
uint16_t hangiPID[3] = {65001,65002,65003};	
uint16_t hangiMotor[8] = {65005,65006,65007,65008,65010,65011,65012,65013};
uint16_t katsayiGiden = 65030;
uint16_t hangiYon[12] = { 65051, 65052, 65053, 65054, 65055, 65056, 65041, 65042, 65043, 65044, 65045, 65046 };
uint16_t gidenAciKontrol = 60000;
uint16_t hizMotor_1=0,hizMotor_2=0,hizMotor_3=0,hizMotor_4=0,hizKameraSagSol=0,hizKameraYukariAsagi=0;
int16_t GhizMotor_1=0,GhizMotor_2=0,GhizMotor_3=0,GhizMotor_4=0;
char veri_bilgisi[50];
uint8_t zamanlayici_denetim=0;
float toplamHata_X=0,toplamHata_Y=0,toplamHata_Z=0;
float oncekiHata_X=0,oncekiHata_Y=0,oncekiHata_Z=0;
float Yuvarlanma_P=0,Yuvarlanma_I=0,Yuvarlanma_D=0,Yunuslama_P=0,Yunuslama_I=0,Yunuslama_D=0,Yonelme_P=0,Yonelme_I=0,Yonelme_D=0;
uint8_t tx_data[20];
uint8_t sayac_um7=0,durum_um7=0,i_u=0;
uint8_t veri_um7[30];
uint8_t packet_has_data = 0,packet_is_batch = 0, batch_length = 0, packet_type = 0, data_length = 0, address = 0 , data_index = 0 , checksum1 = 0, checksum0 = 0,sonuc_um7=1;
uint16_t checksum10=0;			
uint16_t computed_checksum=0;
uint16_t roll_u=0,pitch_u=0,yaw_u=0,roll_rate_u=0,pitch_rate_u=0,yaw_rate_u=0;
uint8_t rx_data[30],rx_data_length=0;
uint8_t regRollPitch = 0x70;
uint8_t regYaw = 0x71;


void UM7_VeriGonder(uint8_t Adres)
{  
  uint8_t chksum0 = 0, chksum1 = 0;
  uint16_t chksum = 0; 
	

	//Veri okuma islemi icin gerekli toplam kontrol bilgisini olusturuyoruz
  chksum = 's' + 'n' + 'p' + (PT_IS_BATCH | PT_BATCH_LEN) + Adres; 
  chksum1 = chksum >> 8;
  chksum0 = chksum & 0xFF;
  
	//Herbir veriyi ayri ayri yolluyoruz. Veri sirasi bilgi sayfasindan gorulebilir
	TM_USART_Putc(USART3,'s');
	TM_USART_Putc(USART3,'n');
	TM_USART_Putc(USART3,'p');
	TM_USART_Putc(USART3,(PT_IS_BATCH | PT_BATCH_LEN));
	TM_USART_Putc(USART3,Adres);
	TM_USART_Putc(USART3,chksum1);
	TM_USART_Putc(USART3,chksum0);
	
}

uint8_t checksum()
{
	int i=0;
	checksum10  = checksum1 << 8;	// toplam kontrol bilgisini 16 bit haline getiriyoruz
	checksum10 |= checksum0;
	computed_checksum = 's' + 'n' + 'p' + packet_type + address; //Gelen verinin toplam kontrol bilgisi olusturuluyor
	for (i = 0; i < data_length; i++){
		computed_checksum += rx_data[i];
	}
	if (checksum10 == computed_checksum) //Gelenle bizim olusturdugumuz toplam kontrol bilgisi kiyaslaniyor
	{ //Her ikisi esitse veri dogru bir sekilde okundu demektir
		roll_u = ((rx_data[0] << 8) | rx_data[1]);
		pitch_u = ((rx_data[2] << 8) | rx_data[3]);
		yaw_u = ((rx_data[4] << 8) | rx_data[5]);
		
		//Gelen aci 360'dan buyukse isaretin eksi oldugunu anliyoruz
		if(roll_u > 32767)
		{
			roll_u = 65536 - roll_u;
			Yuvarlanma = -(roll_u / 91.02222);
		}
		else
			Yuvarlanma = roll_u / 91.02222;
		
		if(pitch_u > 32767)
		{
			pitch_u = 65536 - pitch_u;
			Yunuslama = -(pitch_u / 91.02222);
		}
		else
			Yunuslama = pitch_u / 91.02222;
		
		if(yaw_u > 32767)
		{
			yaw_u = 65536 - yaw_u;
			Yonelme = -(yaw_u / 91.02222);
		}
		else
			Yonelme = yaw_u / 91.02222;
		
		//Sifir konumundaki farkliliktan oturu on deger ekliyoruz
		Yuvarlanma += Yuvarlanma_Ondeger;
		Yunuslama += Yunuslama_Ondeger;
		Yonelme += Yonelme_Ondeger;
		
		return 1;
	} 
	else 
		return 0;
	
}

uint8_t Um7_Denetim(uint8_t veri)
{
	//Herbir okuma islemi sonunda gelen verinin cesidine gore okumanin ne durumda oldugu tespit ediliyor
	//Akis sirasi 's' 'n' 'p' PT Adres Veri 
	switch(durum_um7)
	{
		case 0:
			if(veri=='s')
				durum_um7 = 1;
			else
				durum_um7 = 0;
			return 0;
		case 1:
			if(veri=='n')
				durum_um7 = 2;
			else
				durum_um7 = 0;
			return 0;
		case 2:
			if(veri=='p')
				durum_um7 = 3;
			else
				durum_um7 = 0;
			return 0;
		case 3:
			durum_um7 = 4;
			packet_type = veri;
			packet_has_data = (packet_type >> 7) & 0x01;
			packet_is_batch = (packet_type >> 6) & 0x01;
			batch_length    = (packet_type >> 2) & 0x0F;
			if (packet_has_data)
			{
				if (packet_is_batch)			
					data_length = 4 * batch_length;	// Each data packet is 4 bytes long
				else 
					data_length = 4;
			} 
			else 
				data_length = 0;
			return 0;
		case 4:
			durum_um7 = 5;
			address = veri;
			data_index = 0;
			return 0;
		case 5:
			rx_data[data_index] = veri;
			data_index++;
			if(data_index >= data_length)
				durum_um7 = 6;
			return 0;
		case 6:
			durum_um7 = 7;
			checksum1 = veri;
			return 0;
		case 7:
			durum_um7 = 0;
			checksum0 = veri;
			return checksum();
		default:
			return 0;
	}
}

void TM_USART3_ReceiveHandler(uint8_t alinanVeri)
{	
	//USART3 kesmesiyle sensor verileri yukaridaki fonksiyona yollaniyor 
	sonuc_um7 = Um7_Denetim(alinanVeri);
	
}

void TM_USART1_ReceiveHandler(uint8_t alinanVeri)
{	
		//Bilgisayar ile islemcinin haberlesme kismi
		gelenVeri[sayac] = alinanVeri; //Karsidan gelen veriler iki bayt oldugu icin iki adet veri bekliyoruz
		sayac++;

		if(sayac==2) //Iki adet bayt gelmisse
		{
				sayac=0; //Gelecek islemler icin sayac sifirlaniyor
			
				veri = ( gelenVeri[1] << 8 ) + gelenVeri[0]; //16 bitlik denetim degerimizi aliyoruz
			
				if((veri==hangiPID[0]) || (veri==hangiPID[1]) || (veri==hangiPID[2]) || (veri==hangiMotor[0]) || (veri==hangiMotor[1]) || (veri==hangiMotor[2]) || (veri==hangiMotor[3]) || (veri==hangiMotor[4]) || (veri==hangiMotor[5]) || (veri==hangiMotor[6]) || (veri==katsayiGiden)) //Gelen bilgi hangi islem bilgisi verisi mi kontrol ediliyor
						denetim = veri; //PID, motor, kamera hiz degerlerini almak icin oncelikle hangi verinin gelecegi tespit ediliyor 
				else if((veri==hangiYon[0]) || (veri==hangiYon[1]) || (veri==hangiYon[2]) || (veri==hangiYon[3]) || (veri==hangiYon[4]) || (veri==hangiYon[5]) || (veri==hangiYon[6])  || (veri==hangiMotor[7]))
				{   //Motorun hareket yonu bilgisi geliyor. Bu bilgiye gore istenilen eksenin referans degeri degistiriliyor. 
						//Bu sayede istenilen yone dogru hareket gerceklesiyor
						if(veri==hangiYon[0])
						{
									//Ileri
									refYunuslama= -HareketAcisi;
									TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
									TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
									TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
									TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
						else if(veri==hangiYon[1])
						{
									//Sol
									refYuvarlanma = -HareketAcisi;
									TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
									TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
									TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
									TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
						else if(veri==hangiYon[2])
						{
									//Sag
									refYuvarlanma = HareketAcisi;
									TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
									TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
									TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
									TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
						else if(veri==hangiYon[3])
						{
									//Geri
									refYunuslama = HareketAcisi;
									TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
									TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
									TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
									TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
						else if(veri==hangiYon[4])
						{
									//Saat yonunun tersi
									refYonelme = HareketAcisi;
									TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
									TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
									TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
									TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
						else if(veri==hangiYon[5])
						{
									//Saat yonu
									refYonelme = -HareketAcisi;
									TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
									TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
									TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
									TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
						else if(veri==hangiYon[6])
						{
									//Sifir konumu
									refYunuslama = 0;
									refYuvarlanma = 0;
									refYonelme = 0;
									TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
									TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
									TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
									TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
						else if(veri==hangiMotor[7])
						{
									//Baglanti kuruldugunda butun motorlar sifirlaniyor
									hizMotor_1 = 0;
									hizMotor_2 = 0;
									hizMotor_3 = 0;
									hizMotor_4 = 0;
									TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
									TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
									TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
									TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
				}
				else if(veri == gidenAciKontrol)
				{
						//Veri alma durumunda calistiriliyor anlik olarak aci ve motor degerleri yollaniyor
						sprintf(veri_bilgisi,"X:%.2f Y:%.2f Z:%.2f M1:%d M2:%d M3:%d M4:%d\n",Yuvarlanma,Yunuslama,Yonelme,GhizMotor_1,GhizMotor_2,GhizMotor_3,GhizMotor_4);
						TM_USART_Puts(USART1,veri_bilgisi);
				}
				else
				{
						if(denetim == hangiPID[0]) //Eger yuvarlanma pid degerleri ise 
						{
								//Veriler sirayla aliniyor
								if(k==0)
								{
										k=1;
										Yuvarlanma_P = veri / 1000.0;
								}
								else if(k==1)
								{
										k=2;
										Yuvarlanma_I = veri / 1000.0;
								}
								else if(k==2)
								{
										k=0;
										Yuvarlanma_D = veri / 1000.0;
								}	
						}
						else if(denetim == hangiPID[1]) //Eger yunuslama pid degerleri ise 
						{
								if(k==0)
								{
										k=1;
										Yunuslama_P = veri / 1000.0;
								}
								else if(k==1)
								{
										k=2;
										Yunuslama_I = veri / 1000.0;
								}
								else if(k==2)
								{
										k=0;
										Yunuslama_D = veri / 1000.0;
								}	

						}
						else if(denetim == hangiPID[2]) //Eger yonelme pid degerleri ise 
						{
								if(k==0)
								{
										k=1;
										Yonelme_P = veri / 1000.0;
								}
								else if(k==1)
								{
										k=2;
										Yonelme_I = veri / 1000.0;
								}
								else if(k==2)
								{
										k=0;
										Yonelme_D = veri / 1000.0;
								}	
						}
						else if(denetim==hangiMotor[4]) // Motor hizlari
						{
								hizMotor_1 = veri;
								hizMotor_2 = veri;
								hizMotor_3 = veri;
								hizMotor_4 = veri;
								TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
								TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
								TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
								TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
						else if(denetim==hangiMotor[0])
						{
								//Birinci motor icin hiz degeri
								hizMotor_1 = veri;
								TM_SERVO_SetDegrees(&Motor1, hizMotor_1);
						}
						else if(denetim==hangiMotor[1])
						{
								//Ikinci motor icin hiz degeri
								hizMotor_2 = veri;
								TM_SERVO_SetDegrees(&Motor2, hizMotor_2);
						}
						else if(denetim==hangiMotor[2])
						{
								//Ucuncu motor icin hiz degeri
								hizMotor_3 = veri;
								TM_SERVO_SetDegrees(&Motor3, hizMotor_3);
						}
						else if(denetim==hangiMotor[3])
						{
								//Dorduncu motor icin hiz degeri
								hizMotor_4 = veri;
								TM_SERVO_SetDegrees(&Motor4, hizMotor_4);
						}
						else if(denetim==hangiMotor[5])
						{
								//Kamera sag ve sol yonlerinde hareket degeri
								hizKameraSagSol = veri;
							  TM_SERVOKamera_SetDegrees(&Motor_KameraSS, hizKameraSagSol);
						}
						else if(denetim==hangiMotor[6])
						{
								//Kamera yukari ve asagi yonlerinde hareket degeri
								hizKameraYukariAsagi = veri;
								TM_SERVOKamera_SetDegrees(&Motor_KameraYA, hizKameraYukariAsagi);	
						}
						else if(denetim==katsayiGiden) //Eger motor katsayi verileri ise
						{
								//Herbir motor, -farkli hiz degerleri icin- katsayilar ayri ayri yollaniyor
								if(k==0)
								{
										k=1;
										motor1_Katsayi1 = veri / 1000.0;
								}
								else if(k==1)
								{
										k=2;
										motor2_Katsayi1 = veri / 1000.0;
								}
								else if(k==2)
								{
										k=3;
										motor3_Katsayi1 = veri / 1000.0;
								}	
								else if(k==3)
								{
										k=4;
										motor4_Katsayi1 = veri / 1000.0;
								}
								else if(k==4)
								{
										k=5;
										motor1_Katsayi2 = veri / 1000.0;
								}
								else if(k==5)
								{
										k=6;
										motor2_Katsayi2 = veri / 1000.0;
								}
								else if(k==6)
								{
										k=7;
										motor3_Katsayi2 = veri / 1000.0;
								}	
								else if(k==7)
								{
										k=0;
										motor4_Katsayi2 = veri / 1000.0;
								}	
						}	
				}
		}
}


void TIM4_IRQHandler(void)
{
		//Sensor olcumunun gerceklesecegi zaman araligini tespit icin zamanlayici kullandik
		if (TIM_GetITStatus(TIM4, TIM_IT_Update) != RESET)
		{
				TIM_ClearITPendingBit(TIM4,TIM_IT_Update);
				zamanlayici_denetim = 1;		
		}
}


void Zamanlayici4_Baslat(void) 
{
    TIM_TimeBaseInitTypeDef TIM_BaseStruct;
		NVIC_InitTypeDef NVIC_InitStructure;
	
		NVIC_InitStructure.NVIC_IRQChannel = TIM4_IRQn;
		NVIC_InitStructure.NVIC_IRQChannelPreemptionPriority = 2;
		NVIC_InitStructure.NVIC_IRQChannelSubPriority = 0;
		NVIC_InitStructure.NVIC_IRQChannelCmd = ENABLE;
		NVIC_Init(&NVIC_InitStructure);
	
    RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM4, ENABLE);
    TIM_BaseStruct.TIM_Prescaler = 8399;
    TIM_BaseStruct.TIM_CounterMode = TIM_CounterMode_Up;

    TIM_BaseStruct.TIM_Period = 199; /* 10 ms */
    TIM_BaseStruct.TIM_ClockDivision = TIM_CKD_DIV1;
    TIM_BaseStruct.TIM_RepetitionCounter = 0;
		TIM_TimeBaseInit(TIM4, &TIM_BaseStruct);

		TIM_ITConfig(TIM4, TIM_IT_Update, ENABLE);
		
    TIM_Cmd(TIM4, ENABLE);
}


void MotorDeger()
{
		//Referans degerine gore hata tespit ediliyor
		hataX = refYuvarlanma - Yuvarlanma;
		hataY = refYunuslama - Yunuslama;
		hataZ = refYonelme - Yonelme;
		
		turevX = hataX - oncekiHata_X;
		integralX = hataX;
	
		//Surekli belirli sayidaki son hatalarin toplami elde ediliyor
	  toplamHata_X = toplamHata_X - veriDizi_integralRoll[sayac_integral];
	  veriDizi_integralRoll[sayac_integral] = integralX;
	  toplamHata_X += integralX;
	
		//Alt ve ust sinir degerleri belirleniyor
		if(toplamHata_X > IntegralToplam)
				toplamHata_X = IntegralToplam;
		else if(toplamHata_X < -IntegralToplam)
				toplamHata_X = -IntegralToplam;
		
		turevY = hataY - oncekiHata_Y;
		integralY = hataY;
		
		toplamHata_Y = toplamHata_Y - veriDizi_integralPitch[sayac_integral];
	  veriDizi_integralPitch[sayac_integral] = integralY;
	  toplamHata_Y += integralY;
		
		if(toplamHata_Y > IntegralToplam)
				toplamHata_Y = IntegralToplam;
		else if(toplamHata_Y < -IntegralToplam)
				toplamHata_Y = -IntegralToplam;
		
		turevZ = hataZ - oncekiHata_Z;
		integralZ = hataZ;
		
		toplamHata_Z = toplamHata_Z - veriDizi_integralYaw[sayac_integral];
	  veriDizi_integralYaw[sayac_integral] = integralZ;
	  toplamHata_Z += integralZ;
		
		if(toplamHata_Z > IntegralToplam)
				toplamHata_Z = IntegralToplam;
		else if(toplamHata_Z < -IntegralToplam)
				toplamHata_Z = -IntegralToplam;
	
		sayac_integral++;
			
		//Belirlenen hizlara gore farkli motor katsayilari kullanilabiliyor. Bununla birlikte herbiri icin ayni bir katsayi da kullanmak mumkun
		//Acilarin isaretine gore herbir motora arti veya eksi olarak gonderiliyor
		if(hizMotor_1 <= 230)
			GhizMotor_1 = motor1_Katsayi1*(hizMotor_1 + hataX*Yuvarlanma_P + toplamHata_X * Yuvarlanma_I + turevX * Yuvarlanma_D + hataY * Yunuslama_P + toplamHata_Y * Yunuslama_I + turevY * Yunuslama_D + (hataZ * Yonelme_P - toplamHata_Z * Yunuslama_I - turevZ * Yunuslama_D));
		else
			GhizMotor_1 = motor1_Katsayi2*(hizMotor_1 + hataX*Yuvarlanma_P + toplamHata_X * Yuvarlanma_I + turevX * Yuvarlanma_D + hataY * Yunuslama_P + toplamHata_Y * Yunuslama_I + turevY * Yunuslama_D + (hataZ * Yonelme_P - toplamHata_Z * Yunuslama_I - turevZ * Yunuslama_D));

		if(hizMotor_2 <= 230)
			GhizMotor_2 = motor2_Katsayi1*(hizMotor_2 - hataX*Yuvarlanma_P - toplamHata_X * Yuvarlanma_I - turevX * Yuvarlanma_D + hataY * Yunuslama_P + toplamHata_Y * Yunuslama_I + turevY * Yunuslama_D - (hataZ * Yonelme_P + toplamHata_Z * Yunuslama_I + turevZ * Yunuslama_D));
		else
			GhizMotor_2 = motor2_Katsayi2*(hizMotor_2 - hataX*Yuvarlanma_P - toplamHata_X * Yuvarlanma_I - turevX * Yuvarlanma_D + hataY * Yunuslama_P + toplamHata_Y * Yunuslama_I + turevY * Yunuslama_D - (hataZ * Yonelme_P + toplamHata_Z * Yunuslama_I + turevZ * Yunuslama_D));
		
		if(hizMotor_3 <= 230)
			GhizMotor_3 = motor3_Katsayi1*(hizMotor_3 + hataX*Yuvarlanma_P + toplamHata_X * Yuvarlanma_I + turevX * Yuvarlanma_D - hataY * Yunuslama_P - toplamHata_Y * Yunuslama_I - turevY * Yunuslama_D - (hataZ * Yonelme_P + toplamHata_Z * Yunuslama_I + turevZ * Yunuslama_D));
		else
			GhizMotor_3 = motor3_Katsayi2*(hizMotor_3 + hataX*Yuvarlanma_P + toplamHata_X * Yuvarlanma_I + turevX * Yuvarlanma_D - hataY * Yunuslama_P - toplamHata_Y * Yunuslama_I - turevY * Yunuslama_D - (hataZ * Yonelme_P + toplamHata_Z * Yunuslama_I + turevZ * Yunuslama_D));
			
		if(hizMotor_4 <= 230)
			GhizMotor_4 = motor4_Katsayi1*(hizMotor_4 - hataX*Yuvarlanma_P - toplamHata_X * Yuvarlanma_I - turevX * Yuvarlanma_D - hataY * Yunuslama_P - toplamHata_Y * Yunuslama_I - turevY * Yunuslama_D + (hataZ * Yonelme_P - toplamHata_Z * Yunuslama_I - turevZ * Yunuslama_D));
		else
			GhizMotor_4 = motor4_Katsayi2*(hizMotor_4 - hataX*Yuvarlanma_P - toplamHata_X * Yuvarlanma_I - turevX * Yuvarlanma_D - hataY * Yunuslama_P - toplamHata_Y * Yunuslama_I - turevY * Yunuslama_D + (hataZ * Yonelme_P - toplamHata_Z * Yunuslama_I - turevZ * Yunuslama_D));

		//Motorlara alt ve ust sinir degerleri veriliyor
		if(GhizMotor_1 > 400)
			GhizMotor_1 = 400;
		if(GhizMotor_2 > 400)
			GhizMotor_2 = 400;
		if(GhizMotor_3 > 400)
			GhizMotor_3 = 400;
		if(GhizMotor_4 > 400)
			GhizMotor_4 = 400;
		
		if(GhizMotor_1 < 0)
			GhizMotor_1 = 0;
		if(GhizMotor_2 < 0)
			GhizMotor_2 = 0;
		if(GhizMotor_3 < 0)
			GhizMotor_3 = 0;
		if(GhizMotor_4 < 0)
			GhizMotor_4 = 0;
		
		TM_SERVO_SetDegrees(&Motor1, GhizMotor_1);
		TM_SERVO_SetDegrees(&Motor2, GhizMotor_2);
		TM_SERVO_SetDegrees(&Motor3, GhizMotor_3);
		TM_SERVO_SetDegrees(&Motor4, GhizMotor_4);
		
		oncekiHata_X = hataX;
		oncekiHata_Y = hataY;
		oncekiHata_Z = hataZ;
		
		if(sayac_integral == 10)
				sayac_integral = 0;
	
}

int main (void) {
		
    SystemInit();
	
		/* USART1, 9600 baud, TX: PB6 */
    TM_USART_Init(USART1, TM_USART_PinsPack_2, 9600);
	
		/* USART3, 115200 baud, TX: PB10 */
    TM_USART_Init(USART3, TM_USART_PinsPack_1, 115200);
	
    TM_DELAY_Init();
    
		/* MOTOR 1, TIM3, Channel 1, Pinspack 1 = PB4 */
    TM_SERVO_Init(&Motor1, TIM3, TM_PWM_Channel_1, TM_PWM_PinsPack_2);
	
		/* MOTOR 1, TIM3, Channel 2, Pinspack 1 = PB5 */
    TM_SERVO_Init(&Motor2, TIM3, TM_PWM_Channel_2, TM_PWM_PinsPack_2);
	
		/* MOTOR 1, TIM3, Channel 3, Pinspack 1 = PC8 */
    TM_SERVO_Init(&Motor3, TIM3, TM_PWM_Channel_3, TM_PWM_PinsPack_2);
		
		/* MOTOR 1, TIM3, Channel 4, Pinspack 1 = PC9 */
    TM_SERVO_Init(&Motor4, TIM3, TM_PWM_Channel_4, TM_PWM_PinsPack_2);
		
		/*MOTOR SS Pinspack = PA2*/
		TM_SERVOKamera_Init(&Motor_KameraSS, TIM9, TM_PWM_Channel_1, TM_PWM_PinsPack_1);
		
		/*MOTOR YA Pinspack = PA3*/
		TM_SERVOKamera_Init(&Motor_KameraYA, TIM9, TM_PWM_Channel_2, TM_PWM_PinsPack_1);
		
		//Usart, zamanlayici onceliklerini ayarladigimiz kisim
		NVIC_PriorityGroupConfig(NVIC_PriorityGroup_4);

		Zamanlayici4_Baslat();

		//Esc kalibrasyonu icin motorun azami ve asgari hiz degerlerini 2 sn aralikla gonderiyoruz
		TM_SERVO_SetDegrees(&Motor1, SERVO_AZAMI);
		TM_SERVO_SetDegrees(&Motor2, SERVO_AZAMI);
		TM_SERVO_SetDegrees(&Motor3, SERVO_AZAMI);
		TM_SERVO_SetDegrees(&Motor4, SERVO_AZAMI);
		Delayms(2000);
		TM_SERVO_SetDegrees(&Motor1, 0);
		TM_SERVO_SetDegrees(&Motor2, 0);
		TM_SERVO_SetDegrees(&Motor3, 0);
		TM_SERVO_SetDegrees(&Motor4, 0);
		Delayms(2000);
    	
		TM_WATCHDOG_Init(TM_WATCHDOG_Timeout_2s);
	  UM7_VeriGonder(regRollPitch);
		
    while (1) 
		{
				//Belirlenen sure zarfinda okuma islemi gerceklestirilip bu degerler motorlara gonderiliyor
				if(zamanlayici_denetim == 1)
				{
					zamanlayici_denetim = 0;
					UM7_VeriGonder(regRollPitch);
					MotorDeger();
				}

				//Ani bir kilitlenme durumunda wathcdog zamanlayici devreye giriyor
        TM_WATCHDOG_Reset();
    }
}
