using System.Collections.Generic;
using UnityEngine;

public class Book : MonoBehaviour
{
    public Dictionary<string, List<string>> _openingBook = new Dictionary<string, List<string>>
    {
        { "Scotch Gambit", new List<string> { "e4 e5", "Nf3 Nc6", "d4 exd4", "Bc4 Be7", "c3 dxc3", "Qd5 Nh6", "Bxh6 O-O", "Bc1 Nb4", "Qd1 c2" } },
        { "Evans' Gambit", new List<string> { "e4 e5", "Nf3 Nc6", "Bc4 Bc5", "b4 Bxb4", "c3 Be7", "d4 Na5", "Be2 Nf6", "dxe5 Nxe4"} },
        { "Italian Game", new List<string> { "e4 e5", "Nf3 Nc6", "Bc4 Bc5", "c3 Nf6", "d4 exd4", "cxd4 Bb4", "Nc3 Nxe4", "O-O Nxc3"} },
        { "Two Knights'", new List<string> { "e4 e5", "Nf3 Nc6", "Bc4 Nf6", "d4 exd4", "e5 d5", "Bb5 Ne4", "Nxd4 Bc5", "Be3 O-O", "Bxc6 bxc6"} },
        { "Petroff Defense", new List<string> { "e4 e5", "Nf3 Nf6", "d4 Nxe4", "Bd3 d5", "Nxe5 Nd7", "Nc3 Nxe5", "dxe5 Bb4"} },
    };
}
