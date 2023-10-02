using System;

namespace JJMasterData.Commons.Validations;

internal static class ValidateBrazil
{
    public static bool ValidIE(string pUF, string pInscr)
    {
        bool retorno = false;
        string strBase;
        string strBase2;
        string strOrigem;
        string strDigito1;
        string strDigito2;
        int intPos;
        int intValor;
        int intSoma = 0;
        int intResto;
        int intNumero;
        int intPeso = 0;

        strBase = "";
        strBase2 = "";
        strOrigem = "";

        if (pInscr.Trim().Equals(""))
        {
            return false;
        }

        if ((pInscr.Trim().ToUpper() == "ISENTO"))
            return true;

        for (intPos = 1; intPos <= pInscr.Trim().Length; intPos++)
        {
            if ((("0123456789P".IndexOf(pInscr.Substring((intPos - 1), 1), 0, StringComparison.OrdinalIgnoreCase) + 1) > 0))
                strOrigem = (strOrigem + pInscr.Substring((intPos - 1), 1));
        }

        switch (pUF.ToUpper())
        {
            case "AC":
                #region

                strBase = (strOrigem.Trim() + "00000000000").Substring(0, 11);

                if (strBase.Substring(0, 2) == "01")
                {
                    intSoma = 0;
                    intPeso = 4;

                    for (intPos = 1; (intPos <= 11); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        if (intPeso == 1) intPeso = 9;

                        intSoma += intValor * intPeso;

                        intPeso--;
                    }

                    intResto = (intSoma % 11);
                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    intSoma = 0;
                    strBase = (strOrigem.Trim() + "000000000000").Substring(0, 12);
                    intPeso = 5;

                    for (intPos = 1; (intPos <= 12); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        if (intPeso == 1) intPeso = 9;

                        intSoma += intValor * intPeso;
                        intPeso--;
                    }

                    intResto = (intSoma % 11);
                    strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                    strBase2 = (strBase.Substring(0, 12) + strDigito2);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }
                #endregion

                break;

            case "AL":

                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                if ((strBase.Substring(0, 2) == "24"))
                {
                    //24000004-8
                    //98765432
                    intSoma = 0;
                    intPeso = 9;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intSoma += intValor * intPeso;
                        intPeso--;
                    }

                    intSoma = (intSoma * 10);
                    intResto = (intSoma % 11);

                    strDigito1 = ((intResto == 10) ? "0" : Convert.ToString(intResto)).Substring((((intResto == 10) ? "0" : Convert.ToString(intResto)).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "AM":

                #region
                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                intSoma = 0;
                intPeso = 9;

                for (intPos = 1; (intPos <= 8); intPos++)
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                    intSoma += intValor * intPeso;
                    intPeso--;
                }

                intResto = (intSoma % 11);

                if (intSoma < 11)
                    strDigito1 = (11 - intSoma).ToString();
                else
                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                strBase2 = (strBase.Substring(0, 8) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;
                #endregion

                break;

            case "AP":

                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                intPeso = 9;

                if ((strBase.Substring(0, 2) == "03"))
                {
                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        intSoma += intValor * intPeso;
                        intPeso--;
                    }

                    intResto = (intSoma % 11);
                    intValor = (11 - intResto);

                    strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "BA":

                #region

                if (strOrigem.Length == 8)
                    strBase = (strOrigem.Trim() + "00000000").Substring(0, 8);
                else if (strOrigem.Length == 9)
                    strBase = (strOrigem.Trim() + "00000000").Substring(0, 9);

                if ((("0123458".IndexOf(strBase.Substring(0, 1), 0, StringComparison.OrdinalIgnoreCase) + 1) > 0) && strBase.Length == 8)
                {
                    #region

                    intSoma = 0;

                    for (intPos = 1; (intPos <= 6); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        if (intPos == 1) intPeso = 7;

                        intSoma += intValor * intPeso;
                        intPeso--;
                    }


                    intResto = (intSoma % 10);
                    strDigito2 = ((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Length - 1));


                    strBase2 = strBase.Substring(0, 7) + strDigito2;

                    if (strBase2 == strOrigem)
                        retorno = true;

                    if (retorno)
                    {
                        intSoma = 0;
                        intPeso = 0;

                        for (intPos = 1; (intPos <= 7); intPos++)
                        {
                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            if (intPos == 7)
                                intValor = int.Parse(strBase.Substring((intPos), 1));

                            if (intPos == 1) intPeso = 8;

                            intSoma += intValor * intPeso;
                            intPeso--;
                        }


                        intResto = (intSoma % 10);
                        strDigito1 = ((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 6) + strDigito1 + strDigito2);

                        if ((strBase2 == strOrigem))
                            retorno = true;
                    }

                    #endregion
                }
                else if ((("679".IndexOf(strBase.Substring(0, 1), 0, StringComparison.OrdinalIgnoreCase) + 1) > 0) && strBase.Length == 8)
                {
                    #region

                    intSoma = 0;

                    for (intPos = 1; (intPos <= 6); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        if (intPos == 1) intPeso = 7;

                        intSoma += intValor * intPeso;
                        intPeso--;
                    }


                    intResto = (intSoma % 11);
                    strDigito2 = ((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Length - 1));


                    strBase2 = strBase.Substring(0, 7) + strDigito2;

                    if (strBase2 == strOrigem)
                        retorno = true;

                    if (retorno)
                    {
                        intSoma = 0;
                        intPeso = 0;

                        for (intPos = 1; (intPos <= 7); intPos++)
                        {
                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            if (intPos == 7)
                                intValor = int.Parse(strBase.Substring((intPos), 1));

                            if (intPos == 1) intPeso = 8;

                            intSoma += intValor * intPeso;
                            intPeso--;
                        }


                        intResto = (intSoma % 11);
                        strDigito1 = ((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 6) + strDigito1 + strDigito2);

                        if ((strBase2 == strOrigem))
                            retorno = true;
                    }

                    #endregion
                }
                else if ((("0123458".IndexOf(strBase.Substring(1, 1), 0, StringComparison.OrdinalIgnoreCase) + 1) > 0) && strBase.Length == 9)
                {
                    #region
                    /* Segundo digito */
                    //1000003
                    //8765432
                    intSoma = 0;


                    for (intPos = 1; (intPos <= 7); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                        if (intPos == 1) intPeso = 8;

                        intSoma += intValor * intPeso;
                        intPeso--;
                    }

                    intResto = (intSoma % 10);
                    strDigito2 = ((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((10 - intResto))).Length - 1));

                    strBase2 = strBase.Substring(0, 8) + strDigito2;

                    if (strBase2 == strOrigem)
                        retorno = true;

                    if (retorno)
                    {
                        //1000003 6
                        //9876543 2
                        intSoma = 0;
                        intPeso = 0;

                        for (intPos = 1; (intPos <= 8); intPos++)
                        {
                            intValor = int.Parse(strBase.Substring((intPos - 1), 1));

                            if (intPos == 8)
                                intValor = int.Parse(strBase.Substring((intPos), 1));

                            if (intPos == 1) intPeso = 9;

                            intSoma += intValor * intPeso;
                            intPeso--;
                        }


                        intResto = (intSoma % 10);
                        strDigito1 = ((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto == 0) ? "0" : Convert.ToString((11 - intResto))).Length - 1));

                        strBase2 = (strBase.Substring(0, 7) + strDigito1 + strDigito2);

                        if ((strBase2 == strOrigem))
                            retorno = true;
                    }

                    #endregion
                }

                #endregion

                break;

            case "CE":

                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                intSoma = 0;

                for (intPos = 1; (intPos <= 8); intPos++)
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                    intValor = (intValor * (10 - intPos));
                    intSoma = (intSoma + intValor);
                }

                intResto = (intSoma % 11);
                intValor = (11 - intResto);

                if ((intValor > 9))
                    intValor = 0;

                strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));

                strBase2 = (strBase.Substring(0, 8) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;

                #endregion

                break;

            case "DF":

                #region

                strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 13);

                if ((strBase.Substring(0, 3) == "073"))
                {
                    intSoma = 0;
                    intPeso = 2;

                    for (intPos = 11; (intPos >= 1); intPos = (intPos + -1))
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * intPeso);
                        intSoma = (intSoma + intValor);
                        intPeso = (intPeso + 1);

                        if ((intPeso > 9))
                            intPeso = 2;
                    }

                    intResto = (intSoma % 11);
                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                    strBase2 = (strBase.Substring(0, 11) + strDigito1);
                    intSoma = 0;
                    intPeso = 2;

                    for (intPos = 12; (intPos >= 1); intPos = (intPos + -1))
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * intPeso);
                        intSoma = (intSoma + intValor);
                        intPeso = (intPeso + 1);

                        if ((intPeso > 9))
                            intPeso = 2;
                    }

                    intResto = (intSoma % 11);
                    strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                    strBase2 = (strBase.Substring(0, 12) + strDigito2);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "ES":

                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                intSoma = 0;

                for (intPos = 1; (intPos <= 8); intPos++)
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                    intValor = (intValor * (10 - intPos));
                    intSoma = (intSoma + intValor);
                }

                intResto = (intSoma % 11);
                strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                strBase2 = (strBase.Substring(0, 8) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;

                #endregion

                break;

            case "GO":

                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                if ((("10,11,15".IndexOf(strBase.Substring(0, 2), 0, StringComparison.OrdinalIgnoreCase) + 1) > 0))
                {
                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * (10 - intPos));
                        intSoma = (intSoma + intValor);
                    }

                    intResto = (intSoma % 11);

                    if ((intResto == 0))
                        strDigito1 = "0";
                    else if ((intResto == 1))
                    {
                        intNumero = int.Parse(strBase.Substring(0, 8));
                        strDigito1 = (((intNumero >= 10103105) && (intNumero <= 10119997)) ? "1" : "0").Substring(((((intNumero >= 10103105) && (intNumero <= 10119997)) ? "1" : "0").Length - 1));
                    }
                    else
                        strDigito1 = Convert.ToString((11 - intResto)).Substring((Convert.ToString((11 - intResto)).Length - 1));

                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "MA":

                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                if ((strBase.Substring(0, 2) == "12"))
                {
                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * (10 - intPos));
                        intSoma = (intSoma + intValor);
                    }

                    intResto = (intSoma % 11);
                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "MT":
                #region

                strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);
                intSoma = 0;
                intPeso = 2;

                for (intPos = 10; intPos >= 1; intPos = (intPos + -1))
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                    intValor = (intValor * intPeso);
                    intSoma = (intSoma + intValor);
                    intPeso = (intPeso + 1);

                    if ((intPeso > 9))
                        intPeso = 2;
                }

                intResto = (intSoma % 11);
                strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                strBase2 = (strBase.Substring(0, 10) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;

                #endregion

                break;
            case "MS":
                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                if ((strBase.Substring(0, 2) == "28"))
                {
                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * (10 - intPos));
                        intSoma = (intSoma + intValor);
                    }

                    intResto = (intSoma % 11);
                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "MG":

                #region

                strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 13);
                strBase2 = (strBase.Substring(0, 3) + ("0" + strBase.Substring(3, 8)));
                intNumero = 2;
                for (intPos = 1; (intPos <= 12); intPos++)
                {

                    intValor = int.Parse(strBase2.Substring((intPos - 1), 1));
                    intNumero = ((intNumero == 2) ? 1 : 2);
                    intValor = (intValor * intNumero);
                    if ((intValor > 9))
                    {
                        strDigito1 = String.Format("{0:00}", intValor);
                        intValor = (int.Parse(strDigito1.Substring(0, 1)) + int.Parse(strDigito1.Substring((strDigito1.Length - 1))));

                    }
                    intSoma = (intSoma + intValor);
                }

                intValor = intSoma;
                while ((String.Format("{0:000}", intValor).Substring((String.Format("{0:000}", intValor).Length - 1)) != "0"))
                {
                    intValor = (intValor + 1);
                }
                strDigito1 = String.Format("{0:00}", (intValor - intSoma)).
                    Substring((String.Format("{0:00}", (intValor - intSoma)).Length - 1));

                strBase2 = (strBase.Substring(0, 11) + strDigito1);
                intSoma = 0;
                intPeso = 2;
                for (intPos = 12; (intPos >= 1); intPos = (intPos + -1))
                {
                    intValor = int.Parse(strBase2.Substring((intPos - 1), 1));
                    intValor = (intValor * intPeso);
                    intSoma = (intSoma + intValor);
                    intPeso = (intPeso + 1);
                    if ((intPeso > 11))
                    {
                        intPeso = 2;
                    }
                }

                intResto = (intSoma % 11);
                strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).
                    Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                strBase2 = (strBase2 + strDigito2);
                if ((strBase2 == strOrigem))
                {
                    retorno = true;
                }

                #endregion

                break;

            case "PA":

                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                if ((strBase.Substring(0, 2) == "15"))
                {
                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * (10 - intPos));
                        intSoma = (intSoma + intValor);
                    }

                    intResto = (intSoma % 11);
                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "PB":

                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                intSoma = 0;

                for (intPos = 1; (intPos <= 8); intPos++)
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                    intValor = (intValor * (10 - intPos));
                    intSoma = (intSoma + intValor);
                }

                intResto = (intSoma % 11);
                intValor = (11 - intResto);

                if ((intValor > 9))
                    intValor = 0;

                strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));
                strBase2 = (strBase.Substring(0, 8) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;

                #endregion

                break;

            case "PE":
                #region

                strBase = (strOrigem.Trim() + "0000000").Substring(0, 7);

                intSoma = 0;
                for (intPos = 0; intPos < 7; intPos++)
                {
                    intValor = Convert.ToInt32(strBase.Substring(intPos, 1));
                    intValor *= (9 - intPos);
                    intSoma += intValor;
                }

                intResto = intSoma % 11;

                if (intResto.Equals(0) | intResto.Equals(1))
                {
                    strDigito1 = "0";
                }
                else
                {
                    strDigito1 = (11 - intResto).ToString();
                }

                intSoma = 0;
                for (intPos = 0; intPos < 8; intPos++)
                {
                    intValor = Convert.ToInt32((strBase + strDigito1).Substring(intPos, 1));
                    intValor *= (10 - intPos);
                    intSoma += intValor;
                }

                intResto = intSoma % 11;

                if (intResto.Equals(0) | intResto.Equals(1))
                {
                    strDigito2 = "0";
                }
                else
                {
                    strDigito2 = (11 - intResto).ToString();
                }

                if (strBase + strDigito1 + strDigito2 == strOrigem)
                {
                    retorno = true;
                }


                #endregion

                break;

            case "PI":
                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                intSoma = 0;

                for (intPos = 1; (intPos <= 8); intPos++)
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                    intValor = (intValor * (10 - intPos));
                    intSoma = (intSoma + intValor);
                }

                intResto = (intSoma % 11);
                strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                strBase2 = (strBase.Substring(0, 8) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;

                #endregion

                break;

            case "PR":
                #region

                strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);
                intSoma = 0;
                intPeso = 2;

                for (intPos = 8; (intPos >= 1); intPos = (intPos + -1))
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                    intValor = (intValor * intPeso);
                    intSoma = (intSoma + intValor);
                    intPeso = (intPeso + 1);

                    if ((intPeso > 7))
                        intPeso = 2;
                }

                intResto = (intSoma % 11);
                strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                strBase2 = (strBase.Substring(0, 8) + strDigito1);
                intSoma = 0;
                intPeso = 2;

                for (intPos = 9; (intPos >= 1); intPos = (intPos + -1))
                {
                    intValor = int.Parse(strBase2.Substring((intPos - 1), 1));
                    intValor = (intValor * intPeso);
                    intSoma = (intSoma + intValor);
                    intPeso = (intPeso + 1);

                    if ((intPeso > 7))
                        intPeso = 2;
                }

                intResto = (intSoma % 11);
                strDigito2 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                strBase2 = (strBase2 + strDigito2);

                if ((strBase2 == strOrigem))
                    retorno = true;

                #endregion

                break;

            case "RJ":
                #region

                strBase = (strOrigem.Trim() + "00000000").Substring(0, 8);
                intSoma = 0;
                intPeso = 2;

                for (intPos = 7; (intPos >= 1); intPos = (intPos + -1))
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                    intValor = (intValor * intPeso);
                    intSoma = (intSoma + intValor);
                    intPeso = (intPeso + 1);

                    if ((intPeso > 7))
                        intPeso = 2;
                }

                intResto = (intSoma % 11);
                strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                strBase2 = (strBase.Substring(0, 7) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;

                #endregion

                break;

            case "RN": //Verficar com 10 digitos
                #region

                if (strOrigem.Length == 9)
                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                else if (strOrigem.Length == 10)
                    strBase = (strOrigem.Trim() + "000000000").Substring(0, 10);

                if ((strBase.Substring(0, 2) == "20") && strBase.Length == 9)
                {
                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * (10 - intPos));
                        intSoma = (intSoma + intValor);
                    }

                    intSoma = (intSoma * 10);
                    intResto = (intSoma % 11);
                    strDigito1 = ((intResto > 9) ? "0" : Convert.ToString(intResto)).Substring((((intResto > 9) ? "0" : Convert.ToString(intResto)).Length - 1));
                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }
                else if (strBase.Length == 10)
                {
                    intSoma = 0;

                    for (intPos = 1; (intPos <= 9); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * (11 - intPos));
                        intSoma = (intSoma + intValor);
                    }

                    intSoma = (intSoma * 10);
                    intResto = (intSoma % 11);
                    strDigito1 = ((intResto > 10) ? "0" : Convert.ToString(intResto)).Substring((((intResto > 10) ? "0" : Convert.ToString(intResto)).Length - 1));
                    strBase2 = (strBase.Substring(0, 9) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "RO":
                #region
                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                strBase2 = strBase.Substring(3, 5);
                intSoma = 0;

                for (intPos = 1; (intPos <= 5); intPos++)
                {
                    intValor = int.Parse(strBase2.Substring((intPos - 1), 1));
                    intValor = (intValor * (7 - intPos));
                    intSoma = (intSoma + intValor);
                }

                intResto = (intSoma % 11);
                intValor = (11 - intResto);

                if ((intValor > 9))
                    intValor = (intValor - 10);

                strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));
                strBase2 = (strBase.Substring(0, 8) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;
                #endregion

                break;

            case "RR":
                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);

                if ((strBase.Substring(0, 2) == "24"))
                {
                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor *= intPos;
                        intSoma += intValor;
                    }

                    intResto = (intSoma % 9);
                    strDigito1 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));
                    strBase2 = (strBase.Substring(0, 8) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "RS":
                #region

                strBase = (strOrigem.Trim() + "0000000000").Substring(0, 10);
                intNumero = int.Parse(strBase.Substring(0, 3));

                if (((intNumero > 0) && (intNumero < 468)))
                {
                    intSoma = 0;
                    intPeso = 2;

                    for (intPos = 9; (intPos >= 1); intPos = (intPos + -1))
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * intPeso);
                        intSoma = (intSoma + intValor);
                        intPeso = (intPeso + 1);

                        if ((intPeso > 9))
                            intPeso = 2;
                    }

                    intResto = (intSoma % 11);
                    intValor = (11 - intResto);

                    if ((intValor > 9))
                        intValor = 0;

                    strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));
                    strBase2 = (strBase.Substring(0, 9) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

            case "SC":
                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                intSoma = 0;

                for (intPos = 1; (intPos <= 8); intPos++)
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                    intValor = (intValor * (10 - intPos));
                    intSoma = (intSoma + intValor);
                }

                intResto = (intSoma % 11);
                strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                strBase2 = (strBase.Substring(0, 8) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;
                #endregion

                break;

            case "SE":
                #region

                strBase = (strOrigem.Trim() + "000000000").Substring(0, 9);
                intSoma = 0;

                for (intPos = 1; (intPos <= 8); intPos++)
                {
                    intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                    intValor = (intValor * (10 - intPos));
                    intSoma = (intSoma + intValor);
                }

                intResto = (intSoma % 11);
                intValor = (11 - intResto);

                if ((intValor > 9))
                    intValor = 0;

                strDigito1 = Convert.ToString(intValor).Substring((Convert.ToString(intValor).Length - 1));
                strBase2 = (strBase.Substring(0, 8) + strDigito1);

                if ((strBase2 == strOrigem))
                    retorno = true;

                #endregion

                break;

            case "SP":
                #region

                if ((strOrigem.Substring(0, 1) == "P"))
                {
                    strBase = (strOrigem.Trim() + "0000000000000").Substring(0, 13);
                    strBase2 = strBase.Substring(1, 8);
                    intSoma = 0;
                    intPeso = 1;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos), 1));
                        intValor = (intValor * intPeso);
                        intSoma = (intSoma + intValor);
                        intPeso = (intPeso + 1);

                        if ((intPeso == 2))
                            intPeso = 3;

                        if ((intPeso == 9))
                            intPeso = 10;
                    }

                    intResto = (intSoma % 11);
                    strDigito1 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));
                    strBase2 = (strBase.Substring(0, 9) + (strDigito1 + strBase.Substring(10, 3)));
                }
                else
                {
                    strBase = (strOrigem.Trim() + "000000000000").Substring(0, 12);
                    intSoma = 0;
                    intPeso = 1;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * intPeso);
                        intSoma = (intSoma + intValor);
                        intPeso = (intPeso + 1);

                        if ((intPeso == 2))
                            intPeso = 3;

                        if ((intPeso == 9))
                            intPeso = 10;
                    }

                    intResto = (intSoma % 11);
                    strDigito1 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));
                    strBase2 = (strBase.Substring(0, 8) + (strDigito1 + strBase.Substring(9, 2)));
                    intSoma = 0;
                    intPeso = 2;

                    for (intPos = 11; (intPos >= 1); intPos = (intPos + -1))
                    {
                        intValor = int.Parse(strBase.Substring((intPos - 1), 1));
                        intValor = (intValor * intPeso);
                        intSoma = (intSoma + intValor);
                        intPeso = (intPeso + 1);

                        if ((intPeso > 10))
                            intPeso = 2;
                    }

                    intResto = (intSoma % 11);
                    strDigito2 = Convert.ToString(intResto).Substring((Convert.ToString(intResto).Length - 1));
                    strBase2 = (strBase2 + strDigito2);
                }

                if ((strBase2 == strOrigem))
                    retorno = true;

                #endregion

                break;

            case "TO":
                #region

                strBase = (strOrigem.Trim() + "00000000000").Substring(0, 11);

                if ((("01,02,03,99".IndexOf(strBase.Substring(2, 2), 0, StringComparison.OrdinalIgnoreCase) + 1) > 0))
                {
                    strBase2 = (strBase.Substring(0, 2) + strBase.Substring(4, 6));
                    intSoma = 0;

                    for (intPos = 1; (intPos <= 8); intPos++)
                    {
                        intValor = int.Parse(strBase2.Substring((intPos - 1), 1));
                        intValor = (intValor * (10 - intPos));
                        intSoma = (intSoma + intValor);
                    }

                    intResto = (intSoma % 11);
                    strDigito1 = ((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Substring((((intResto < 2) ? "0" : Convert.ToString((11 - intResto))).Length - 1));
                    strBase2 = (strBase.Substring(0, 10) + strDigito1);

                    if ((strBase2 == strOrigem))
                        retorno = true;
                }

                #endregion

                break;

        }

        return retorno;
    }

}
