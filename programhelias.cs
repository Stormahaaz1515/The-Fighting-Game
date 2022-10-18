static int InitStats(string stat, Role role) //Donne les stats en fonction des classes.
{
    if (stat == "health") // Attribution des stats de sant�.
        switch (role)
        {
            case (Role)0:
                return 4;
            case (Role)1:
                return 5;
            case (Role)2:
                return 4;
            case (Role)3:
                return 4;
        }
    else if (stat == "damage") // Attribution des stats de d�gats.
        switch (role)
        {
            case (Role)0:
                return 1;
            case (Role)1:
                return 1;
            case (Role)2:
                return 3;
            case (Role)3:
                return 2;
        }
    else if (stat == "heal") // Attribution des stats de self healing.
        switch (role)
        {
            case (Role)0:
                return 3;
            case (Role)1:
                return 0;
            case (Role)2:
                return 0;
            case (Role)3:
                return 1;
        }
    else if (stat == "damageThroughtDef") // Attribution des stats de d�gats � travers la d�fense.
        switch (role)
        {
            case (Role)0:
                return 0;
            case (Role)1:
                return 1;
            case (Role)2:
                return 0;
            case (Role)3:
                return 1;
        }
    return 0;
}
#endregion

#region GameLoop
static void LetsPlay(ref List<Player> players, bool simulationIA) // Fonction assurant le d�roulement d'une manche.
{
    int nbManche = 1;
    while (players.Count > 1)
    {
        if (!simulationIA)
        {
            DisplayLine(nbManche); // S�paration de deux manches par une �p�e BADASS.
        }

        if (players.Count > 2)
            ResetAllTargets(players);

        for (int i = 0; i < players.Count; i++) // Identification du joueur dont c'est le tour, affichage de son nom et de ses PV.
        {
            if (players[i].specialUsed > 0)
                players[i].specialUsed -= 1; //A chaque d�but de tour, le cooldown baisse.

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.Write("Tour de " + players[i].name + "     PV : ");

            DisplayHearts(players[i]);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;

            if (players.Count > 2) //si plus de 2 joueurs, possibilit� de choisir la cible � attaquer.
            {
                int target = GetTarget(i, players);

                players[i].target = target;
                players[target].targeted.Add(i);

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(players[i].name + " cible " + players[target].name + ".");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
            } //si plus de 2 joueurs, possibilit� de choisir la cible � attaquer.
            else InitTarget(ref players);

            if (!players[i].isAI) // r�cup�ration de l'action du joueur.
            {
                GetAction(players[i]);

                if (!simulationIA)
                    Thread.Sleep(1000);

                ExecuteSpecial(players[i], players); //Permet d'ex�cuter les sp�ciales (Comme Sharingan de Kakashit).
            } // r�cup�ration de l'action du joueur.
            else // Si le joueur est un ordinateur, la d�cision de son choix se fait dans cette condition.
            {
                if (!simulationIA)
                    Thread.Sleep(1000);
                IaChoice(players[i], players); // Fonction permettant � l'ordinateur de faire un choix.
            } // Si le joueur est un ordinateur, la d�cision de son choix se fait dans cette condition.

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(players[i].name + " (" + players[i].role + ")" + " choisit de : " + players[i].choice + " (Cible : " + players[players[i].target].name + ").");
            Console.ForegroundColor = ConsoleColor.Green;
        }

        ResolutionAction(ref players); // Fonction permettant d'attribuer � la variable TurnStats les PV � ajouter ou a retirer au joueur apr�s application des choix de tous les joueurs du tour.

        Console.WriteLine();
        foreach (var player in players.ToList()) // Affichage d'une synth�se du d�roulement du tour pr�c�dant pour chacun des joueurs restants.
        {
            Console.ForegroundColor = ConsoleColor.White;

            player.health += player.turnStats.applyDmg; //Applique les d�gats.

            if (player.health > player.maxHealth) //Limite le heal � la vie max.
                player.health = player.maxHealth;

            if (player.turnStats.applyDmg != 0) //affiche si un joueur a perdu ou gagn� des PV pendant la manche.
            {
                string winOrLose;

                if (player.turnStats.applyDmg > 0)
                    winOrLose = " a gagn� ";
                else
                    winOrLose = " a perdu ";

                Console.WriteLine(player.name + "(" + player.role + ")" + winOrLose + Math.Abs(player.turnStats.applyDmg) + " PV.");
            } //affiche si un joueur a perdu ou gagn� des PV pendant la manche.

            if (player.health <= 0) //annonce la mort d'un joueur puis le supprime de la liste.
            {
                if (!simulationIA)
                    Console.WriteLine("Le joueur " + player.name + " s'est cass� le poignet tr�s fort, et est d�c�d�...");

                players.Remove(player);
            } //annonce la mort d'un joueur puis le supprime de la liste.
        }

        Console.WriteLine();

        foreach (var player in players.ToList())
        {
            Console.Write("   " + player.name + "(" + player.role + ")  PV : ");

            DisplayHearts(player);

            Console.ForegroundColor = ConsoleColor.White;
        }
        Console.WriteLine();
        Console.WriteLine();
        nbManche++;
    }

    nbSimu--; // compteur de simulations pour les IA, d�cr�mente apr�s chaque simulation.
}

static void ResolutionAction(ref List<Player> players) //Fonction permettant de d�finir pour chaque joueur le total de PV perdus ou gagn� apr�s que chaque joueur ait choisi son action.
{
    for (int i = 0; i < players.Count; i++)
    {
        players[i].turnStats = new PlayerTurnStats(); // Remise � z�ro des stats d'application des d�gats.

        if (players[i].choice == Action.Special)
            SpecialAttack(players[i].role, players[i]); // Attribution des capacit�s sp�ciales en fonction du role.

        ComputeDamages(players[i], players, ref players[i].turnStats); // Attribution des stats pour le tour � chaucun des joueurs.
    }

    for (int i = 0; i < players.Count; i++) // R�solution des d�gats � infliger ou � soigner en fonction des choix des joueurs et de leurs cibles.
    {
        players[i].turnStats.applyDmg += players[i].turnStats.heal;

        foreach (int element in players[i].targeted)
        {
            if (players[i].choice != Action.Defend) //Si on se d�fend pas prend tous les d�gats.
            {
                players[i].turnStats.applyDmg -= players[element].turnStats.damage + players[element].turnStats.damageThroughtDef;
            }
            else //Si on se d�fend, prend que les d�gats � travers la d�fense.
            {
                players[i].turnStats.applyDmg -= players[element].turnStats.damageThroughtDef;
            }

            if (players[i].choice == Action.Reflect) //Sp� du damager.
            {
                if (players[i].role != Role.Kakashit)
                    players[element].turnStats.applyDmg -= players[element].turnStats.damage + players[element].turnStats.damageThroughtDef;
                else //Kakashit ne renvoie pas autant de d�gats que le damager de base.
                    players[element].turnStats.applyDmg -= (players[element].turnStats.damage + players[element].turnStats.damageThroughtDef > 0) ? 1 : 0;
            }
        }
    }
}

static void ComputeDamages(Player player, List<Player> players, ref PlayerTurnStats stats) // Attribution des capacit�s sp�ciales aux classes de base.
{
    switch (player.choice)
    {
        case (Action.Attack):
            stats.damage = player.damage;
            break;
        case (Action.Healing):
            stats.heal = player.heal;
            break;
        case (Action.Berserk):
            if (player.role == Role.Tank)
                stats.damage = player.damage;
            if (player.role == Role.Kakashit)
                stats.damage = 0;
            stats.damageThroughtDef = player.damageThroughtDef;
            stats.heal -= 1;
            break;
    }

    SetSpecialUsed(player);
}