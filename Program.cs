#region Choices
static Role GetRole() //Fonction pour choisir le role du joueur.
{
    Role roleJoueur;

    Console.WriteLine(chooseChar);

    while (!Enum.TryParse(Console.ReadLine(), out roleJoueur)) //répète la question si commande invalide.
    {
        Console.WriteLine("\n" + errorMsg);
        Console.WriteLine(chooseChar);
    }

    if ((int)roleJoueur < 0 || (int)roleJoueur > 3)
    {
        Console.WriteLine("\n" + errorMsg);
        return GetRole();
    }

    return roleJoueur;
}

static bool GetGameDifficulty()//Pour choisir le niveau de difficulté entre une IA qui fait ses chois aléatoirement
{
    Console.WriteLine("[Quelle difficulté de jeu voulez-vous ?] \n1 - Newbie Player (Les choix de l'IA sont aléatoires, et les capacités spéciales n'ont pas de cooldown). \n2 - Hardcore Player (Les choix de l'IA sont scriptés, et les capacités spéciales ont un cooldown de deux tours).");

    int iaSmartChoice;
    while (!int.TryParse(Console.ReadLine(), out iaSmartChoice))
    {
        Console.WriteLine("\n" + errorMsg);
        Console.WriteLine("[Quelle difficulté de jeu voulez-vous ?] \n1 - Newbie Player \n2 - Hardcore Player");
    }

    if (iaSmartChoice < 1 || iaSmartChoice > 2)
    {
        Console.WriteLine(errorMsg);
        return GetGameDifficulty();
    }

    return iaSmartChoice == 2;
}

static void GetAction(Player p) //Permet au joueur de choisir son action.
{
    Console.WriteLine(chooseAbility);

    while (!Enum.TryParse(Console.ReadLine(), out p.choice)) //répète la question si commande invalide
    {
        Console.WriteLine(errorMsg);
        Console.WriteLine(chooseAbility);
    }

    if ((int)p.choice > 3 || (int)p.choice <= 0)
    {
        GetAction(p);
        return;
    }
    else if (p.specialUsed > 0 && (int)p.choice == 3) //Vérifie si on peut utiliser l'attaque spéciale
    {
        Console.WriteLine("Vous pourrez utiliser votre attaque spéciale dans : " + p.specialUsed + " tour(s).");
        GetAction(p);
        return;
    }

    SetSpecialUsed(p);
}

static int GetTarget(int playerIndex, List<Player> players) // Permet au joueur de choisir sa cible.
{
    int target;

    Console.WriteLine(chooseTarget);
    ShowAllPlayers(players);

    while (!int.TryParse(Console.ReadLine(), out target))
    {
        Console.WriteLine(errorMsg);
        Console.WriteLine(chooseTarget);
        ShowAllPlayers(players);
    }

    if (target == playerIndex || target < 0 || target >= players.Count)
    {
        Console.WriteLine(errorMsg);

        return GetTarget(playerIndex, players);
    }

    return target;
}

static Role RandomRole() //Choisit aléatoirement la classe du personnage de l'IA
{
    Random random = new Random();

    return (Role)random.Next(0, 4);
}

static void IaChoice(Player p, List<Player> players)
{
    if (!isHardGame) //Si IA en mode facile : choix aléatoire
    {
        Random random = new Random();

        p.choice = (Action)random.Next(1, 4);
    }
    else
    {
        Action opChoice = players[p.target].choice;
        p.choice = Action.None;

        int futureOpHealth = players[p.target].health;
        int possibleDamages = 0;
        bool isOpDefending = false;

        if (opChoice == Action.Defend)
            isOpDefending = true;
        else if (opChoice == Action.Attack)
        {
            possibleDamages += players[p.target].damage;
        }
        else //Si Attaque Spéciale de l'opposant
        {
            switch (players[p.target].role) //Action en fonction du role de l'IA et des actions du joueur.
            {
                case (Role.Healer):
                    if (p.role == Role.Tank && p.health > 1 && p.specialUsed == 0)
                        p.choice = Action.Special;
                    else
                        p.choice = Action.Attack;
                    break;
                case (Role.Tank):
                    if (p.specialUsed > 0) break;

                    possibleDamages += players[p.target].damage + players[p.target].damageThroughtDef;
                    futureOpHealth -= 1;
                    break;
                case (Role.Damager):
                    int val = new Random().Next(0, 100);
                    if (val < 50)
                        p.choice = Action.Defend; //Se défend une fois sur deux, évite les games interminables.

                    if (p.specialUsed == 0)
                    {
                        switch (p.role)
                        {
                            case (Role.Healer):
                                p.choice = Action.Special;
                                break;
                            case (Role.Kakashit):
                                p.choice = Action.Healing;
                                break;
                        }
                    }
                    break;
                case (Role.Kakashit):
                    switch (players[p.target].choice)
                    {
                        case (Action.Healing):
                            p.choice = Action.Attack;
                            break;
                        case (Action.Berserk):
                            if (p.role == Role.Healer && p.specialUsed == 0)
                                p.choice = Action.Special;
                            else
                                p.choice = Action.Attack;
                            break;
                        case (Action.Reflect):
                            if (p.role == Role.Healer && p.specialUsed == 0)
                                p.choice = Action.Special;
                            else
                                p.choice = Action.Defend;
                            break;
                    }
                    break;
            }
        }
        //Si l'IA n'a pas choisi d'action dans les conditions précédentes, on choisit en fonction des valeurs récupérées.
        if (p.choice == Action.None)
        {
            p.choice = Action.Attack;

            if (futureOpHealth - p.damage <= 0 && !isOpDefending)
                p.choice = Action.Attack;
            else if (p.health - possibleDamages <= 0)
            {
                int val = new Random().Next(0, 100);
                if (val < 50)
                    p.choice = Action.Defend; //Se défend une fois sur deux, évite les games interminables.

                if (p.specialUsed == 0)
                {
                    switch (p.role)
                    {
                        case (Role.Healer):
                            p.choice = Action.Special;
                            break;
                        case (Role.Kakashit):
                            p.choice = Action.Healing;
                            break;
                    }
                }
            }
            else if (isOpDefending)
            {
                int val = new Random().Next(0, 100);

                if (val < 50)
                    p.choice = Action.Defend;

                if (p.specialUsed == 0)
                {
                    switch (p.role)
                    {
                        case (Role.Tank):
                            if (p.health > 1)
                                p.choice = Action.Special;
                            break;
                        case (Role.Healer):
                            p.choice = Action.Special;
                            break;
                        case (Role.Kakashit):
                            if (p.health > 1)
                                p.choice = Action.Berserk;
                            else
                                p.choice = Action.Healing;
                            break;
                    }
                }
            }
            else
            {
                p.choice = Action.Attack;

                if (p.specialUsed == 0)
                {
                    switch (p.role)
                    {
                        case (Role.Tank):
                            if (p.health > 2)
                                p.choice = Action.Special;
                            break;
                    }
                }

            }
        }
    }

    SetSpecialUsed(p);

    ExecuteSpecial(p, players);
}