using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
//using static ConsoleApp1.Program;

namespace ConsoleApp1
{
    class Program
    {
        //messages de base nécessaires à l'éxécution d'une manche et à la présentation du jeu, stockés dans des strings pour plus d'organisation.
        public static string title = "████████╗██╗░░██╗███████╗  ███████╗██╗░██████╗░██╗░░██╗████████╗██╗███╗░░██╗░██████╗░\n╚══██╔══╝██║░░██║██╔════╝  ██╔════╝██║██╔════╝░██║░░██║╚══██╔══╝██║████╗░██║██╔════╝░\n░░░██║░░░███████║█████╗░░  █████╗░░██║██║░░██╗░███████║░░░██║░░░██║██╔██╗██║██║░░██╗░\n░░░██║░░░██╔══██║██╔══╝░░  ██╔══╝░░██║██║░░╚██╗██╔══██║░░░██║░░░██║██║╚████║██║░░╚██╗\n░░░██║░░░██║░░██║███████╗  ██║░░░░░██║╚██████╔╝██║░░██║░░░██║░░░██║██║░╚███║╚██████╔╝\n░░░╚═╝░░░╚═╝░░╚═╝╚══════╝  ╚═╝░░░░░╚═╝░╚═════╝░╚═╝░░╚═╝░░░╚═╝░░░╚═╝╚═╝░░╚══╝░╚═════╝░\n \n░██████╗░░█████╗░███╗░░░███╗███████╗\n██╔════╝░██╔══██╗████╗░████║██╔════╝\n██║░░██╗░███████║██╔████╔██║█████╗░░\n██║░░╚██╗██╔══██║██║╚██╔╝██║██╔══╝░░\n╚██████╔╝██║░░██║██║░╚═╝░██║███████╗\n░╚═════╝░╚═╝░░╚═╝╚═╝░░░░░╚═╝╚══════╝\n";
        public static string chooseAbility = "[Que voulez-vous faire ?] \n 1 = Attaquer \n 2 = Se défendre \n 3 = Capacité spéciale";
        public static string chooseChar = "Catégories de personnages disponibles : \n" + DisplayChar() + "\n[Lequel choisissez-vous ?]";
        public static string nbPlayers = "[Combien de joueurs êtes-vous ?]";
        public static string chooseTarget = "[Quel joueur voulez-vous cibler ?]";
        public static string errorMsg = "Veuillez sélectionner un choix valide.";
        public static string chooseName = "[Comment vous appelez-vous ?]";
        public static string credits = "               [CREDITS]               \nHélias Gamonet, Guillaume Patrice, Yuna Bonnifet";
        public static string simulOuJouer = "[Voulez - vous lancer une simulation ou jouer ?] \nSaisissez \"Jouer\" pour jouer et \"Simulation\" pour lancer la simulation.";
        static int nbSimu = 600;

        static bool isHardGame = false; // booléen permettant de définir le niveau de la partie.

        public enum Action // Enumeration permettant de sélectionner une actione à réaliser.
        {
            None,
            Attack = 1,
            Defend = 2,
            Special = 3,
            Reflect,
            Healing,
            Berserk,
            Sharingan
        }

        public enum Role // Enumération permettant de sélectionner les roles et d'y acceder au sein du code.
        {
            Healer = 0,
            Tank = 1,
            Damager = 2,
            Kakashit = 3
        }

        struct PlayerTurnStats // Structure regroupant les statistiques de dégats à appliquer lors d'un tour.
        {
            public int damage;
            public int damageThroughtDef;
            public int heal;
            public int applyDmg;
        }

        class SimulationIA // Regroupement des données de la simulation pour afficher les pourcentages de victoire de chaque matchup.
        {
            public int dmgVsHeal = 0;
            public int dmgVsTank = 0;
            public int healVsTank = 0;
            public int kakaVsDmg = 0;
            public int kakaVsTank = 0;
            public int kakaVsHeal = 0;
        }

        class Player //Regroupement des variables utilisées pour l'implémentation de chaque tour au sein d'une partie.
        {
            public string name = "";
            public Role role; // classe utilisée

            public int target;
            public List<int> targeted = new List<int>(); // liste des joueurs ciblant le joueur.

            public int maxHealth;
            public int health;
            public int damage; // stat de dégat
            public int heal; // stat de PV
            public int damageThroughtDef; // dégats passant à travers l'action défense.

            public Action choice; // choix de l'action pour la manche en cours.
            public PlayerTurnStats turnStats; // stats de dégats à appliquer cette manche après la résolution du tour.

            public int specialUsed; // Ajout d'un cooldown aux capacités spéciales.
            public bool isAI; //Ce joueur est une ia ?

            public Player(Role role, int health, int damage, int heal, int damageThroughtDef, bool isAI) // Constructeur permettant d'initialiser les stats par défaut du joueur en fonction de sa classe.
            {
                this.role = role;
                this.maxHealth = health;
                this.health = health;
                this.damage = damage;
                this.heal = heal;
                this.damageThroughtDef = damageThroughtDef;

                specialUsed = 0;
                this.isAI = isAI;

                this.choice = Action.None;
                turnStats = new PlayerTurnStats();
            }
        }

        static void Main(string[] args) //Structure principale du jeu
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode; //Permet de voir les "emojis" sur la console

            bool simulationIA = false; // booléen permettant de réaliser certaines action en fonction de si la simulation est voulue ou non.
            int nombreJoueur = 0;

            SimulationIA statsIA = new SimulationIA();

            string sim = "";
            Console.ForegroundColor = ConsoleColor.Green; //Met la couleur de texte en vert 
            Console.WriteLine(title);                       // Affichage du titre.
            Console.WriteLine(credits + "\n\n");			// Affichage des crédits.

            //--------------------------- Choix du type de partie ------------------------------------
            List<Player> players = new List<Player>();
            if (!simulationIA)                              //Condition permettant de déterminer si on lance la simulation ou le jeu.
            {
                while (sim != "Simulation" && sim != "Jouer")//boucle pour forcer le joueur à faire un choix entre les deux propositions. 
                {
                    Console.WriteLine(simulOuJouer);
                    sim = Console.ReadLine();
                }

                if (sim == "Simulation")
                    simulationIA = true; // Si le joueur tape "Simulation", le booléen simulationIA passe à true et on lance la simulation.
            }

            //----------------- Choix nombre de joueurs ----------------------
            if (!simulationIA)
            {
                while (nombreJoueur <= 0 || nombreJoueur > 15)
                {
                    Console.WriteLine();
                    Console.WriteLine(nbPlayers);
                    while (!int.TryParse(Console.ReadLine(), out nombreJoueur))// On rentre le nombre de joueurs en vérifiant qu'il rentre un nombre.
                        Console.WriteLine(errorMsg);
                    if (nombreJoueur <= 0)
                        Console.WriteLine("Veuillez sélectionner un minimum de 1 joueur.");
                    else if (nombreJoueur > 15)
                        Console.WriteLine("Vous pouvez sélectionner un maximum de 15 joueurs.");
                }
            }

            Console.WriteLine();

            isHardGame = GetGameDifficulty(); //Choix de la "difficulté" de la partie.

            for (int i = nombreJoueur; i > 0; i--) // Boucle pour initialiser la liste des joueurs, saisir leurs noms ainsi que leurs roles.
            {
                //---------------Choix du nom------------------//
                Console.ForegroundColor = ConsoleColor.Yellow;//change la couleur du texte en jaune.
                Console.WriteLine("         Joueur : " + (nombreJoueur - i + 1));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(chooseName);

                string name = Console.ReadLine();
                Console.WriteLine();

                while (name == "Ordinateur" || name == "") //commande invalide si veut s'appeler "Ordinateur".
                {
                    Console.WriteLine(errorMsg);
                    if (name == "Ordinateur")
                        Console.WriteLine("Vous mentez, vous n'êtes pas un ordinateur.");
                    name = Console.ReadLine();
                    Console.WriteLine();
                }

                //---------------Choix du role------------------//
                Role roleJoueur; // le joueur choisi la classe de son personnage.
                DisplayRole("5"); //Affiche tous les roles
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();

                roleJoueur = GetRole();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();

                Player joueur = new Player(roleJoueur, InitStats("health", roleJoueur), InitStats("damage", roleJoueur), InitStats("heal", roleJoueur), InitStats("damageThroughtDef", roleJoueur), false);
                joueur.name = name;
                players.Add(joueur);
            }

            if (players.Count == 1 || simulationIA) //ajoute une IA comme adversaire, s'il n'y a qu'un seul joueur, ou bien deux en cas de simulation
            {
                if (!simulationIA)
                {
                    Role roleIA = RandomRole();

                    Player IA = new Player(roleIA, InitStats("health", roleIA), InitStats("damage", roleIA), InitStats("heal", roleIA), InitStats("damageThroughtDef", roleIA), true);
                    IA.name = "Ordinateur";

                    players.Add(IA);
                    Console.WriteLine("Vous affrontez un " + roleIA + " joué par une IA.");
                }
                else
                    InitIASimulations(ref players, ref statsIA); //Fonction permettant de conditionner les deux IA pour les faire combattre.
            }

            InitTarget(ref players); //Initialise les cibles automatiquement s'il n'y a qu'un seul joueur

            if (simulationIA && players.Count == 2)
                while (nbSimu > 0)
                {
                    InitIASimulations(ref players, ref statsIA); //Fonction permettant de conditionner les deux IA pour les faire combattre.
                    InitTarget(ref players); //Initialise les cibles automatiquement s'il n'y a qu'un seul joueur
                    LetsPlay(ref players, simulationIA);
                }
            else
                LetsPlay(ref players, simulationIA);

            //Fin de partie
            Console.ForegroundColor = ConsoleColor.Green;
            if (!simulationIA && players.Count != 0)
            {
                DisplayRole(Enum.GetName(typeof(Role), players[0].role)); //affiche image du perso gagnant
                Console.WriteLine(players[0].name + " a survécu, et a vaincu ! ");
            }
            else if (!simulationIA)
                Console.WriteLine("Les combattants se sont entretués, personne n'a gagné... Dommage ! ");

            if (!simulationIA)
                Replay();
            if (simulationIA && nbSimu == 0)
                DisplaySimulation(statsIA);

            Console.ReadKey();
        }
        static void InitTarget(ref List<Player> players) // Initialisation des cibles en cas de partie 1 vs 1
        {
            if (players.Count == 2)
            {
                players[0].targeted.Clear();
                players[1].targeted.Clear();

                players[0].target = 1;
                players[1].target = 0;

                players[0].targeted.Add(1);
                players[1].targeted.Add(0);
            }
        }

        static void InitIASimulations(ref List<Player> players, ref SimulationIA statsIA)
        {
            Role roleIA = default;
            Role roleIA2 = default;

            //Chacun des "if" suivant est une copie conforme du précédent, à la différence que les roles changent pour tester toutes les combinaisons de matchup.
            if (nbSimu >= 500)
            {
                if (players.Count == 1)
                    if (players[0].name == "Ordinateur")
                        statsIA.kakaVsTank++;
                    else if (players.Count == 0) // S'il n'y a pas de joueurs dans la liste, c'est qu'il y a eu match nul, dans ce cas on relance une simulation de plus pour ne pas fausser les chiffres.
                        nbSimu++;

                roleIA = Role.Kakashit;
                roleIA2 = Role.Tank;
            } // 100 simulations du match Kakashit vs Tank.
            else if (nbSimu >= 400)
            {
                if (players.Count == 1)
                    if (players[0].name == "Ordinateur")
                        statsIA.kakaVsHeal++;
                    else if (players.Count == 0)
                        nbSimu++;

                roleIA = Role.Kakashit;
                roleIA2 = Role.Healer;
            } // 100 simulations du match Kakashit vs Heal.
            else if (nbSimu >= 300)
            {
                if (players.Count == 1)
                    if (players[0].name == "Ordinateur")
                        statsIA.kakaVsDmg++;
                    else if (players.Count == 0)
                        nbSimu++;

                roleIA = Role.Kakashit;
                roleIA2 = Role.Damager;
            } // 100 simulations du match Kakashit vs Damager.
            else if (nbSimu >= 200)
            {
                if (players.Count == 1)
                    if (players[0].name == "Ordinateur")
                        statsIA.dmgVsHeal++;
                    else if (players.Count == 0)
                        nbSimu++;

                roleIA = Role.Damager;
                roleIA2 = Role.Healer;
            } // 100 simulations du match Damager vs Heal.
            else if (nbSimu >= 100)
            {
                if (players.Count == 1)
                    if (players[0].name == "Ordinateur")
                        statsIA.dmgVsTank++;
                    else if (players.Count == 0)
                        nbSimu++;

                roleIA = Role.Damager;
                roleIA2 = Role.Tank;
            } // 100 simulations du match Damager vs Tank.
            else if (nbSimu >= 0)
            {
                if (players.Count == 1)
                    if (players[0].name == "Ordinateur")
                        statsIA.healVsTank++;
                    else if (players.Count == 0)
                        nbSimu++;

                roleIA = Role.Healer;
                roleIA2 = Role.Tank;
            } // 100 simulations du match Heal vs Tank.

            players.Clear();

            Player IA = new Player(roleIA, InitStats("health", roleIA), InitStats("damage", roleIA), InitStats("heal", roleIA), InitStats("damageThroughtDef", roleIA), true);
            IA.name = "Ordinateur";
            players.Add(IA);

            Player IA2 = new Player(roleIA2, InitStats("health", roleIA2), InitStats("damage", roleIA2), InitStats("heal", roleIA2), InitStats("damageThroughtDef", roleIA2), true);
            IA2.name = "Ordinateur2";
            players.Add(IA2);
        }



#endregion

        #region Specials
        static void SpecialAttack(Role role, Player joueur) //Attribu les capacités spéciales en fonction de la classe du personnage.
        {
            switch (role)
            {
                case (Role)0:
                    joueur.choice = Action.Healing;
                    break;
                case (Role)1:
                    joueur.choice = Action.Berserk;
                    break;
                case (Role)2:
                    joueur.choice = Action.Reflect;
                    break;
                case (Role)3:
                    joueur.choice = Action.Sharingan;
                    break;
            }
        }
        static void DisplayLine(int nbManche)//Met visuelement une séparation entre les manches, pour plus de lisibilité, avec une GROSSE EPEE BADASS
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n");
            Console.WriteLine("                     MANCHE {0} :", nbManche);
            Console.WriteLine("         /> ________________________________");
            Console.WriteLine("[########[]_________________________________>");
            Console.WriteLine(@"         \>");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
        }
        static void ExecuteSpecial(Player p, List<Player> players)//fonction spéciale pour la capacité spéciale de Kakashit,
                                                                  //car il doit choisir quelle capacité spéciale il copie.
        {
            if (p.choice != Action.Special) return;

            switch (p.role)
            {
                case (Role.Kakashit):

                    bool isAI = false;
                    Role targetRole;

                    if (p.isAI)//si c'est l'ordinateur, capacité spéciale aléatoire
                    {
                        isAI = true;

                        Random random = new Random();

                        targetRole = (Role)random.Next(0, 4);
                    }
                    else //si humain, on donne le choix de la capacité spéciale à copier
                    {
                        Console.WriteLine("\n+------| Sharingan |------+");

                        Console.WriteLine("Choisir une classe à copier.");
                        Console.WriteLine(DisplayChar());

                        while (!Enum.TryParse(Console.ReadLine(), out targetRole))
                        {
                            Console.WriteLine(errorMsg);
                            Console.WriteLine("Choisir une classe à copier.");
                            Console.WriteLine(DisplayChar());
                        }
                    }

                    SpecialAttack(targetRole, p);

                    if (!isAI)
                        Console.WriteLine("\nVous avez copié : " + targetRole + ".");

                    if (p.choice == Action.Sharingan)
                    {
                        p.choice = Action.Special;
                        ExecuteSpecial(p, players);
                    }
                    break;
            }
        }
        #endregion
#endregion

        #region Utils
        static void SetSpecialUsed(Player p) //Le cool down avant de pouvoir réutiliser la capacité spéciale si la difficulté "Hardcore Player" de l'IA est activée
        {
            if ((int)p.choice >= 3)
                p.specialUsed = isHardGame ? 2 : 0;
        }

        static void ResetAllTargets(List<Player> players)
        {
            foreach (Player p in players)
            {
                p.targeted.Clear();
            }
        }

        static int Replay()// affiche les crédits, Propose de rejouer ou quitter 
        {
            string answer = " ";

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Voulez-vous rejouer ? Si oui, tapez 'y', sinon 'n'.");

            while ("n" != (answer = Console.ReadLine()) && "y" != answer)
            {
                Console.Write(errorMsg);
                Console.WriteLine(" Vous pouvez rentrer 'y' ou 'n'.");
            }
            Console.Clear();
            if (answer == "y")
                Main(new string[0]);
            else
                Environment.Exit(0);
            return 0;
        }

        #region Text
        static void DisplayHearts(Player p)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            for (int a = 0; a < p.health; a++) //affiche le nb de PV en "♥"
                Console.Write("♥ ");
        }

        static string DisplayChar() // Affichage des noms des différents rôles disponibles.
        {
            List<string> names = new List<string>();
            string characters = "";

            names = Enum.GetNames(typeof(Role)).ToList();
            for (int i = 0; i < names.Count; i++)
            {
                characters += " " + i + " - ";
                characters += names[i] + "\n";
            }
            return characters;
        }

        static void DisplayRole(string role) //affiche les personnages avec des dessins en ASCII 
        {
            if (role == "5") //Pour présenter les classes en ASCII art
            {
                Console.WriteLine();
                Console.WriteLine(@"		 /\                _____          |\             //                           _,-'|");
                Console.WriteLine(@"                 ||               /_____\          \\           _!_              .||,      ,-'._  |");
                Console.WriteLine(@"   ____ (((+))) _||_         ____[\`---'/]____      \\         /___\            \.`',/     |####\ |");
                Console.WriteLine(@"  /.--.\  .-.  /.||.\       /\ #\_\_____/_/# /\      \\        [+++]            = ,. =     \####| |");
                Console.WriteLine(@" /.,   \\(0.0)// || \\     /  /|\  |   |  /|\  \      \\    _ _\^^^/_ _         / || \    ,-'\#/,'`.");
                Console.WriteLine(@"/;`m;/\ \\|m|//  ||  ;\   /__/ | | |   | | | \__\      \\/ (    '-'  ( )          ,|____,' , ,;' \| |");
                Console.WriteLine(@"|:   \ \__`:`____||__:|  |  |  | | |---| | |  |  |     /( \/ | {&}   /\ \        (3|\    _/|/'   _| |");
                Console.WriteLine(@"|:    \__ \T/ (@~)(~@)|  |__|  \_| |_#_| |_/  |__|       \  / \     / _> )        ||/,-''  | >-'' _,\\");
                Console.WriteLine(@"|:    _/|     |\_\/  :|  //\\  <\ _//^\\_ />  //\\       -`   >:::; -'`-' -.      ||'      ==\ ,-'  ,'");
                Console.WriteLine(@"|:   /  |     |  \   :|  \||/  |\//// \\\\/|  \||/            /:::/         \     ||       |  V \ ,|");
                Console.WriteLine(@"|'  /   |     |   \  '|        |   |   |   |          	     /  /||   { &}   |    ||       |    |   \");
                Console.WriteLine(@" \_/    |     |    \_/         |---|   |---|           	     (  / (\        /     ||       |    \    \");
                Console.WriteLine(@"	|     |                |---|   |---|           	    / /   \'-.___.-'      ||       |     |    \");
                Console.WriteLine(@"	|_____|                |___|   |___|          	 _ / /     \ \            ||       |___,, \_,-'");
                Console.WriteLine(@"	|_____|                /   \   /   \            /___ |      /___|         ||         |_|     )_\");
                Console.WriteLine(@"                              |HHHHH| |HHHHH|                                     ||       ccc/   ccc/ ");
                Console.WriteLine();
                Console.WriteLine("        HEALER                     TANK                      DAMAGER              KAKASHIT, le Ninja Copiteur");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(" Un prêtre un peu             Un tas de muscles        Un épéiste hors pair,        Le ninja copieur du bled,");
                Console.WriteLine("pété, qui n'hésitera        boosté aux stéroïdes,      capable d'identifer le      il peut s'adapter à tous ses");
                Console.WriteLine(" pas à abuser de ses        qui lors de crises de     point faible de n'importe    adversaires en recopiant les");
                Console.WriteLine(" soins pour faire          rage ira au devant du      quel adversaire, et de       bottes secrètes de ceux-ci,");
                Console.WriteLine(" durer la partie...         danger en dépit de	         faire mouche.              mais dans une version ");
                Console.WriteLine("                                sa sécurité.                                            éclatée au sol...\n");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" Force: ⚔    PV: ♥♥♥♥     Force: ⚔   PV: ♥♥♥♥♥       Force:⚔ ⚔ ⚔  PV: ♥♥♥♥        Force: ⚔ ⚔    PV: ♥♥♥♥");
                Console.WriteLine();
                Console.WriteLine("     Récupère 2 PV.          Sacrifie 1 PV pour       Inflige en retour les      Copie une capacité spéciale");
                Console.WriteLine("                             augmenter de 1 sa         dégâts qui lui sont     dont l'efficacité est amoindrie.");
                Console.WriteLine("                            force durant 1 tour.           infligés. ");
            } //Pour présenter les classes en ASCII art

            //pour afficher le dernier personnage gagnant en fonction de son role.
            if (role == "Healer")
            {
                Console.WriteLine();
                Console.WriteLine(@"		 /\");
                Console.WriteLine(@"                 ||");
                Console.WriteLine(@"   ____ (((+))) _||_");
                Console.WriteLine(@"  /.--.\  .-.  /.||.\");
                Console.WriteLine(@" /.,   \\(0.0)// || \\");
                Console.WriteLine(@"/;`m;/\ \\|m|//  ||  ;\");
                Console.WriteLine(@"|:   \ \__`:`____||__:|");
                Console.WriteLine(@"|:    \__ \T/ (@~)(~@)|");
                Console.WriteLine(@"|:    _/|     |\_\/  :|");
                Console.WriteLine(@"|:   /  |     |  \   :|");
                Console.WriteLine(@"|'  /   |     |   \  '|");
                Console.WriteLine(@" \_/    |     |    \_/");
                Console.WriteLine(@"	|     |");
                Console.WriteLine(@"	|_____|");
                Console.WriteLine(@"	|_____|");
                Console.WriteLine();
            }
            if (role == "Tank")
            {
                Console.WriteLine();
                Console.WriteLine(@"          _____");
                Console.WriteLine(@"         /_____\");
                Console.WriteLine(@"    ____[\`---'/]____");
                Console.WriteLine(@"   /\ #\_\_____/_/# /\");
                Console.WriteLine(@" /   /|\  |   |  /|\   \");
                Console.WriteLine(@"/___/ | | |   | | | \___\");
                Console.WriteLine(@"|  |  | | |---| | |  |  |");
                Console.WriteLine(@"|__|  \_| |_#_| |_/  |__|");
                Console.WriteLine(@"//\\  <\ _//^\\_ />  //\\");
                Console.WriteLine(@"\||/  |\//// \\\\/|  \||/");
                Console.WriteLine(@"      |   |   |   |");
                Console.WriteLine(@"      |---|   |---|");
                Console.WriteLine(@"      |---|   |---|");
                Console.WriteLine(@"      |___|   |___|");
                Console.WriteLine(@"      /   \   /   \");
                Console.WriteLine(@"     |HHHHH| |HHHHH|");
                Console.WriteLine();
            }
            if (role == "Damager")
            {

                Console.WriteLine();
                Console.WriteLine(@"|\             //");
                Console.WriteLine(@" \\           _!_");
                Console.WriteLine(@"  \\         /___\");
                Console.WriteLine(@"   \\        [+++]");
                Console.WriteLine(@"    \\    _ _\^^^/_ _");
                Console.WriteLine(@"     \\/ (    '-'  ( )");
                Console.WriteLine(@"     /( \/ | {&}   /\ \");
                Console.WriteLine(@"       \  / \     / _> )");
                Console.WriteLine(@"       -`   >:::; -'`-' -.");
                Console.WriteLine(@"            /:::/         \");
                Console.WriteLine(@"    	   /  /||   { &}   |");
                Console.WriteLine(@"    	  (  / (\         /");
                Console.WriteLine(@"    	  / /   \'-.___.-'");
                Console.WriteLine(@"    	_ / /     \ \");
                Console.WriteLine(@"      /___ |      /___|");
                Console.WriteLine();
            }
            if (role == "Kakashit")
            {
                Console.WriteLine();
                Console.WriteLine(@"              _,-'|");
                Console.WriteLine(@" .||,      ,-'._  |");
                Console.WriteLine(@"\.`',/     |####\ |");
                Console.WriteLine(@"= ,. =     \####| |");
                Console.WriteLine(@"/ || \    ,-'\#/,'`.");
                Console.WriteLine(@"  ,|____,' , ,;' \| |");
                Console.WriteLine(@" (3|\    _/|/'   _| |");
                Console.WriteLine(@"  ||/,-''  | >-'' _,\\");
                Console.WriteLine(@"  ||'      ==\ ,-'  ,'");
                Console.WriteLine(@"  ||       |  V \ ,|");
                Console.WriteLine(@"  ||       |    |   \");
                Console.WriteLine(@"  ||       |    \    \");
                Console.WriteLine(@"  ||       |     |    \");
                Console.WriteLine(@"  ||       |___,, \_,-'");
                Console.WriteLine(@"  ||        |_|     )_\");
                Console.WriteLine(@"  ||       ccc/   ccc/ ");
            }
        }

        static void ShowAllPlayers(List<Player> players) // Affichage des noms des joueurs présents dans la liste.
        {
            for (int i = 0; i < players.Count; i++)
            {
                Console.WriteLine(i + " - " + players[i].name);
            }
        }

        static void DisplaySimulation(SimulationIA statsIA)//Résultats de la simulation
        {
            Console.Clear();
            Console.WriteLine("          Damager   Healer    Tank   Kakashit");
            Console.WriteLine("      +--+--------+--------+--------+--------+");
            Console.WriteLine("         |        |        |        |        |");
            Console.WriteLine(" Damager |   X    |{0}|{1}|{2}|", FillSim(statsIA.dmgVsHeal), FillSim(statsIA.dmgVsTank), FillSim(100 - statsIA.kakaVsDmg));
            Console.WriteLine("         |        |        |        |        |");
            Console.WriteLine("      +--+--------+--------+--------+--------+");
            Console.WriteLine("         |        |        |        |        |");
            Console.WriteLine("  Healer |{0}|   X    |{1}|{2}|", FillSim(100 - statsIA.dmgVsHeal), FillSim(statsIA.healVsTank), FillSim(100 - statsIA.kakaVsHeal));
            Console.WriteLine("         |        |        |        |        |");
            Console.WriteLine("      +--+--------+--------+--------+--------+");
            Console.WriteLine("         |        |        |        |        |");
            Console.WriteLine("   Tank  |{0}|{1}|   X    |{2}|", FillSim(100 - statsIA.dmgVsTank), FillSim(100 - statsIA.healVsTank), FillSim(100 - statsIA.kakaVsTank));
            Console.WriteLine("         |        |        |        |        |");
            Console.WriteLine("      +--+--------+--------+--------+--------+");
            Console.WriteLine("         |        |        |        |        |");
            Console.WriteLine(" Kakashit|{0}|{1}|{2}|   X    |", FillSim(statsIA.kakaVsDmg), FillSim(statsIA.kakaVsHeal), FillSim(statsIA.kakaVsTank));
            Console.WriteLine("         |        |        |        |        |");
            Console.WriteLine("      +--+--------+--------+--------+--------+");
            Console.WriteLine("\nInterprétation de la case Damager/Healer : Sur 100 simulations aléatoires, la classe Damager gagne {0}% des fois contre la classe Healer. Et oui, vous l'avez compris, dans des choix aléatoires la classe Kakashit est tout bonnement éclatée.", statsIA.dmgVsHeal);

            Console.WriteLine("\n\n\nMerci à vous d'avoir lancé la simulation.\n\n\n");
            nbSimu = 600;
            Replay();
        }

        static string FillSpaces(int nb, bool befter, string str) // Fonction remplissant le tableau d'espaces à droite et à gauche du nombre.
        {
            if (befter)
            {
                if (nb == 100)
                    str += "  ";
                else
                    str += "   ";
            }
            else
            {
                if (nb >= 10)
                    str += "  ";
                else
                    str += "   ";
            }
            return str;
        }

        static string FillSim(int nb) // Fonction permettant d'afficher le tableau correctement quelque soit la taille du nombre pour un nombre compris entre 0 et 100.
        {
            string rtn = "";
            int nombre = nb;
            rtn = FillSpaces(nb, true, rtn);

            if (nb > 10)
                while (nb > 10)
                {
                    rtn += (nb / 10).ToString();
                    rtn += (nb % 10).ToString();
                    nb /= 10;
                }
            else
                rtn += nb.ToString();
            rtn += "%";
            rtn = FillSpaces(nombre, false, rtn);
            return rtn;
        }
        #endregion
        #endregion
    }
}
