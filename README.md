# quadcopterControl

Bu projede bir Quadcopterin modellenmesi, kontrolü ve programlanması gerçekleştirilmiştir. Kontrol kartı olarak ARM tabanlı bir mikrodenetleyici olan STM32F4 Discovery kullanılmıştır. Cihaz C# ile tasarlanan bir arayüz yardımıyla Bluetooth aracılığıyla kontrol edilmiştir. Quadcopterin hızı, yönü ve aynı zaman cihaz üzerinde bulunan kameranın yönelimi arayüz üzerinden ayarlanmıştır. Bununla birlikte uçuş ortamının değişmesi durumunda kontrol katsayılarının yeniden gönderilmesi için bir bölüm de yine arayüzde bulunmaktadır.
