Прочти это!
https://docs.google.com/document/d/12cwab45YODC4_BvD84uAc8gblUUg3BT1NP0QhpSWiEg






tick = world.GetCurrentTick | Leo.GetCurrentTick(world)
tick - значение, означающее на каком тике остановилась игра, любой добавленный инпут с этим значением будет обработан со следующий systems.Run()


Основной луп:

//ввод c произойдет тут и будет причислен к N тику


tick = world.GetCurrentTick(); // = N

systems.Run();//состоит из систем  в порядке обновления:
    Systems.Run begin [ 
         Системы обработки инпута с тиком N
         система 1
         система 2
         система 3
         Cистема повыщающая tick до N + 1  (Самая последняя!)
    Systems.Run end   ]

tick = world.GetCurrentTick(); // = N + 1

отправка пинга до сервера со значением N + 1

//ввод произойдет тут и будет причислен к N + 1 тику




[        SP         ][        SP         ]  - unity loop, S - Systems.Run(), P - отпр пинга 
           [iiiiiiii  IIIIIII]              - промежуток в 1 кадр между Systems.Run (N++) 
                                              где может генерироваться любой инпут 
                                              от игрока (клава, мышка, ui)
             