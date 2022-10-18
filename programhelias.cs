static int InitStats(string stat, Role role) //Donne les stats en fonction des classes.
{
    if (stat == "health") // Attribution des stats de santé.
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
    else if (stat == "damage") // Attribution des stats de dégats.
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
    else if (stat == "damageThroughtDef") // Attribution des stats de dégats à travers la défense.
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
static void LetsPlay(ref List<Player> players, bool simulationIA) // Fonction assurant le déroulement d'une manche.
{
    int nbManche = 1;
    while (players.Count > 1)
    {
        if (!simulationIA)
        {
            DisplayLine(nbManche); // Séparation de deux manches par une épée BADASS.
        }

        if (players.Count > 2)
            ResetAllTargets(players);

        for (int i = 0; i < players.Count; i++) // Identification du joueur dont c'est le tour, affichage de son nom et de ses PV.
        {
            if (players[i].specialUsed > 0)
                players[i].specialUsed -= 1; //A chaque début de tour, le cooldown baisse.

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.Write("Tour de " + players[i].name + "     PV : ");

            DisplayHearts(players[i]);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;

            if (players.Count > 2) //si plus de 2 joueurs, possibilité de choisir la cible à attaquer.
            {
                int target = GetTarget(i, players);

                players[i].target = target;
                players[target].targeted.Add(i);

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(players[i].name + " cible " + players[target].name + ".");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
            } //si plus de 2 joueurs, possibilité de choisir la cible à attaquer.
            else InitTarget(ref players);

            if (!players[i].isAI) // récupération de l'action du joueur.
            {
                GetAction(players[i]);

                if (!simulationIA)
                    Thread.Sleep(1000);

                ExecuteSpecial(players[i], players); //Permet d'exécuter les spéciales (Comme Sharingan de Kakashit).
            } // récupération de l'action du joueur.
            else // Si le joueur est un ordinateur, la décision de son choix se fait dans cette condition.
            {
                if (!simulationIA)
                    Thread.Sleep(1000);
                IaChoice(players[i], players); // Fonction permettant à l'ordinateur de faire un choix.
            } // Si le joueur est un ordinateur, la décision de son choix se fait dans cette condition.

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(players[i].name + " (" + players[i].role + ")" + " choisit de : " + players[i].choice + " (Cible : " + players[players[i].target].name + ").");
            Console.ForegroundColor = ConsoleColor.Green;
        }

        ResolutionAction(ref players); // Fonction permettant d'attribuer à la variable TurnStats les PV à ajouter ou a retirer au joueur après application des choix de tous les joueurs du tour.

        Console.WriteLine();
        foreach (var player in players.ToList()) // Affichage d'une synthèse du déroulement du tour précédant pour chacun des joueurs restants.
        {
            Console.ForegroundColor = ConsoleColor.White;

            player.health += player.turnStats.applyDmg; //Applique les dégats.

            if (player.health > player.maxHealth) //Limite le heal à la vie max.
                player.health = player.maxHealth;

            if (player.turnStats.applyDmg != 0) //affiche si un joueur a perdu ou gagné des PV pendant la manche.
            {
                string winOrLose;

                if (player.turnStats.applyDmg > 0)
                    winOrLose = " a gagné ";
                else
                    winOrLose = " a perdu ";

                Console.WriteLine(player.name + "(" + player.role + ")" + winOrLose + Math.Abs(player.turnStats.applyDmg) + " PV.");
            } //affiche si un joueur a perdu ou gagné des PV pendant la manche.

            if (player.health <= 0) //annonce la mort d'un joueur puis le supprime de la liste.
            {
                if (!simulationIA)
                    Console.WriteLine("Le joueur " + player.name + " s'est cassé le poignet très fort, et est décédé...");

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

    nbSimu--; // compteur de simulations pour les IA, décrémente après chaque simulation.
}

static void ResolutionAction(ref List<Player> players) //Fonction permettant de définir pour chaque joueur le total de PV perdus ou gagné après que chaque joueur ait choisi son action.
{
    for (int i = 0; i < players.Count; i++)
    {
        players[i].turnStats = new PlayerTurnStats(); // Remise à zéro des stats d'application des dégats.

        if (players[i].choice == Action.Special)
            SpecialAttack(players[i].role, players[i]); // Attribution des capacités spéciales en fonction du role.

        ComputeDamages(players[i], players, ref players[i].turnStats); // Attribution des stats pour le tour à chaucun des joueurs.
    }

    for (int i = 0; i < players.Count; i++) // Résolution des dégats à infliger ou à soigner en fonction des choix des joueurs et de leurs cibles.
    {
        players[i].turnStats.applyDmg += players[i].turnStats.heal;

        foreach (int element in players[i].targeted)
        {
            if (players[i].choice != Action.Defend) //Si on se défend pas prend tous les dégats.
            {
                players[i].turnStats.applyDmg -= players[element].turnStats.damage + players[element].turnStats.damageThroughtDef;
            }
            else //Si on se défend, prend que les dégats à travers la défense.
            {
                players[i].turnStats.applyDmg -= players[element].turnStats.damageThroughtDef;
            }

            if (players[i].choice == Action.Reflect) //Spé du damager.
            {
                if (players[i].role != Role.Kakashit)
                    players[element].turnStats.applyDmg -= players[element].turnStats.damage + players[element].turnStats.damageThroughtDef;
                else //Kakashit ne renvoie pas autant de dégats que le damager de base.
                    players[element].turnStats.applyDmg -= (players[element].turnStats.damage + players[element].turnStats.damageThroughtDef > 0) ? 1 : 0;
            }
        }
    }
}

static void ComputeDamages(Player player, List<Player> players, ref PlayerTurnStats stats) // Attribution des capacités spéciales aux classes de base.
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