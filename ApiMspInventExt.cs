using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ApisMicrosip
{
    public class ApiMspInventExt
    {
        //function inGetLastErrorCode: Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int inGetLastErrorCode();

        //function inGetLastErrorMessage(ErrorMessage: PChar): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int inGetLastErrorMessage(StringBuilder ErrorMessage);

        //procedure inSetErrorHandling(ExceptionOnError, MessageOnException: Integer); stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern void inSetErrorHandling(int ExceptionOnError, int MessageOnException);

        //// funciones principales
        //function SetDBInventarios(DBHandle: Integer): Integer;  stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int SetDBInventarios(int DBHandle);

        //function NuevaEntrada(ConceptoInId, AlmacenId: Integer; Fecha, Folio,
        //    Descripcion: PChar; CentroCostoId: Integer): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int NuevaEntrada(int ConceptoInId, int AlmacenId,
          string Fecha, string Folio, string Descripcion, int CentroCostoId);

        //function RenglonEntrada(ArticuloId: Integer; Unidades, CostoUnitario,
        //    CostoTotal: Double): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int RenglonEntrada(int ArticuloId, Double Unidades, Double CostoUnitario,
          Double CostoTotal);

        //function RenglonEntradaLotes(ClaveLote, FechaCaducidad: PChar;
        //    Unidades: Double): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int RenglonEntradaLotes(string ClaveLote, string FechaCaducidad,
          Double Unidades);

        //function RenglonEntradaSeries(ClaveSerie: PChar; NumConsecutivos:
        //    Integer): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int RenglonEntradaSeries(string ClaveSerie, int NumConsecutivos);

        //function RenglonEntradaPedimento(ClavePedimento, FechaPedimento,
        //    AduanaPedimento: PChar): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int RenglonEntradaPedimento(string ClavePedimento, string FechaPedimento,
          string AduanaPedimento);

        //function AplicaEntrada: Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int AplicaEntrada();

        //procedure AbortaDoctoInventarios; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern void AbortaDoctoInventarios();

        //// Checar Compatibilidad
        //function GetVersionApiInventariosAsInteger: Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int GetVersionApiInventariosAsInteger();

        //procedure GetVersionApiInventariosAsString(AVersion: PChar); stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern void GetVersionApiInventariosAsString(StringBuilder AVersion);

        //function ChecaCompatibilidadInventarios(HdbMetadatos: Integer): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int ChecaCompatibilidadInventarios(int HdbMetadatos);


        // Funciones de Salidas   **************************
        // function NuevaSalida(ConceptoInId, AlmacenId, AlmacenDestinoId: Integer; Fecha, Folio,
        //    Descripcion: PChar; CentroCostoId: Integer): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int NuevaSalida(int ConceptoInId, int AlmacenId, int AlmacenDestinoId,
          string Fecha, string Folio, string Descripcion, int CentroCostoId);

        // function RenglonSalida(ArticuloId: Integer; Unidades, CostoUnitario,
        //    CostoTotal: Double): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int RenglonSalida(int ArticuloId, Double Unidades, Double CostoUnitario,
          Double CostoTotal);

        // function RenglonSalidaSeries(ClaveSerie: PChar): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int RenglonSalidaSeries(string ClaveSerie);

        // function RenglonSalidaLotes(ClaveLote: PChar; Unidades: Double ): Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int RenglonSalidaLotes(string ClaveLote, Double Unidades);

        //function AplicaSalida: Integer; stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern int AplicaSalida();

        // procedure SetReglasInventarios(ExistenciasNegativas: Integer); stdcall;
        [DllImport("ApiMicrosip.dll", SetLastError = true)]
        public static extern void SetReglasInventarios(int ExistenciasNegativas);
    }

}
