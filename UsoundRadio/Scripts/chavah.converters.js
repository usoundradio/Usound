chavah.converters = {
	rankToColor: function (rank) {
		if (rank < 0) return "DarkRed";
		if (rank == 0) return "Gray";
		if (rank >= 1 && rank < 10) return "LightBlue";
		if (rank >= 10 && rank < 20) return "RoyalBlue";
		if (rank >= 20 && rank < 30) return "MidnightBlue";
		if (rank >= 30 && rank < 40) return "Indigo";
		if (rank >= 40 && rank < 50) return "Orange";
		if (rank >= 50 && rank < 60) return "OrangeRed";
		if (rank >= 60 && rank < 70) return "FireBrick";
		if (rank >= 70 && rank < 80) return "Brown";
		if (rank >= 80 && rank < 90) return "#8C7853";
		if (rank >= 90 && rank < 100) return "GoldenRod";
		if (rank >= 100 && rank < 110) return "Gold";
		if (rank >= 110 && rank < 1000) return "rgb(45, 141, 147)";
		//if (rank >= 1000) return "rgb(247, 49, 247)";
	}
}