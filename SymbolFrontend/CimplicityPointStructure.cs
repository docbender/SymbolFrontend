using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    public class CimplicityPointStructure
    {
        public string PT_ID { get; set; }
        public string ACCESS { get; set; }
        public string ACCESS_FILTER { get; set; }
        public string ACK_TIMEOUT_HI { get; set; }
        public string ACK_TIMEOUT_HIHI { get; set; }
        public string ACK_TIMEOUT_LO { get; set; }
        public string ACK_TIMEOUT_LOLO { get; set; }
        public string ADDR { get; set; }
        public string ADDR_OFFSET { get; set; }
        public string ADDR_TYPE { get; set; }
        public string ALARM_CHANGEAPPROVAL { get; set; }
        public int ALARM_DELAY_HI { get; set; }
        public int ALARM_DELAY_HIHI { get; set; }
        public int ALARM_DELAY_LO { get; set; }
        public int ALARM_DELAY_LOLO { get; set; }
        public string ALARM_DELAY_UNIT_HI { get; set; }
        public string ALARM_DELAY_UNIT_HIHI { get; set; }
        public string ALARM_DELAY_UNIT_LO { get; set; }
        public string ALARM_DELAY_UNIT_LOLO { get; set; }
        public int ALARM_OFF_DELAY_HI { get; set; }
        public int ALARM_OFF_DELAY_HIHI { get; set; }
        public int ALARM_OFF_DELAY_LO { get; set; }
        public int ALARM_OFF_DELAY_LOLO { get; set; }
        public string ALARM_OFF_DELAY_UNIT_HI { get; set; }
        public string ALARM_OFF_DELAY_UNIT_HIHI { get; set; }
        public string ALARM_OFF_DELAY_UNIT_LO { get; set; }
        public string ALARM_OFF_DELAY_UNIT_LOLO { get; set; }
        public string ALARM_PUBLISH { get; set; }
        public string ALM_CLASS { get; set; }
        public string ALM_CRITERIA { get; set; }
        public string ALM_CRITERIA_EX { get; set; }
        public string ALM_DEADBAND { get; set; }
        public int ALM_DELAY { get; set; }
        public string ALM_ENABLE { get; set; }
        public string ALM_HIGH_1 { get; set; }
        public string ALM_HIGH_2 { get; set; }
        public string ALM_HLP_FILE { get; set; }
        public string ALM_LOW_1 { get; set; }
        public string ALM_LOW_2 { get; set; }
        public string ALM_MSG { get; set; }
        public string ALM_OFF_DELAY { get; set; }
        public string ALM_ROUTE_OPER { get; set; }
        public string ALM_ROUTE_SYSMGR { get; set; }
        public string ALM_ROUTE_USER { get; set; }
        public int ALM_SEVERITY { get; set; }
        public int ALM_STR { get; set; }
        public string ALM_TYPE { get; set; }
        public int ALM_UPDATE_VALUE { get; set; }
        public string ANALOG_DEADBAND { get; set; }
        public int BFR_COUNT { get; set; }
        public int BFR_DUR { get; set; }
        public int BFR_EVENT_PERIOD { get; set; }
        public string BFR_EVENT_PT_ID { get; set; }
        public int BFR_EVENT_TYPE { get; set; }
        public int BFR_EVENT_UNITS { get; set; }
        public int BFR_GATE_COND { get; set; }
        public int BFR_SYNC_TIME { get; set; }
        public string CALC_TYPE { get; set; }
        public string CHANGEAPPROVAL { get; set; }
        public string CONV_LIM_HIGH { get; set; }
        public string CONV_LIM_LOW { get; set; }
        public string CONV_TYPE { get; set; }
        public string DELAY_LOAD { get; set; }
        public string DELETE_REQ_HI { get; set; }
        public string DELETE_REQ_HIHI { get; set; }
        public string DELETE_REQ_LO { get; set; }
        public string DELETE_REQ_LOLO { get; set; }
        public string DESC { get; set; }
        public string DEVIATION_PT { get; set; }
        public string DEVICE_ID { get; set; }
        public string DISP_LIM_HIGH { get; set; }
        public string DISP_LIM_LOW { get; set; }
        public string DISP_TYPE { get; set; }
        public string DISP_WIDTH { get; set; }
        public int ELEMENTS { get; set; }
        public string ENG_UNITS { get; set; }
        public string ENUM_ID { get; set; }
        public string EQUATION { get; set; }
        public int EXTRA { get; set; }
        public string FW_CONV_EQ { get; set; }
        public string GR_SCREEN { get; set; }
        public string INIT_VAL { get; set; }
        public string JUSTIFICATION { get; set; }
        public int LEVEL { get; set; }
        public string LOCAL { get; set; }
        public string LOG_ACK_HI { get; set; }
        public string LOG_ACK_HIHI { get; set; }
        public string LOG_ACK_LO { get; set; }
        public string LOG_ACK_LOLO { get; set; }
        public int LOG_DATA { get; set; }
        public int LOG_DATA_HISTORIAN { get; set; }
        public string LOG_DEL_HI { get; set; }
        public string LOG_DEL_HIHI { get; set; }
        public string LOG_DEL_LO { get; set; }
        public string LOG_DEL_LOLO { get; set; }
        public string LOG_GEN_HI { get; set; }
        public string LOG_GEN_HIHI { get; set; }
        public string LOG_GEN_LO { get; set; }
        public string LOG_GEN_LOLO { get; set; }
        public string LOG_RESET_HI { get; set; }
        public string LOG_RESET_HIHI { get; set; }
        public string LOG_RESET_LO { get; set; }
        public string LOG_RESET_LOLO { get; set; }
        public string MAX_STACKED { get; set; }
        public string MEASUREMENT_UNIT_ID { get; set; }
        public int MISC_FLAGS { get; set; }
        public string POLL_AFTER_SET { get; set; }
        public string PRECISION { get; set; }
        public string PROC_ID { get; set; }
        public string PTMGMT_PROC_ID { get; set; }
        public int PT_ENABLED { get; set; }
        public string PT_ORIGIN { get; set; }
        public string PT_SET_INTERVAL { get; set; }
        public string PT_SET_TIME { get; set; }
        public string PT_TYPE { get; set; }
        public string RANGE_HIGH { get; set; }
        public string RANGE_LOW { get; set; }
        public string RAW_LIM_HIGH { get; set; }
        public string RAW_LIM_LOW { get; set; }
        public string REP_TIMEOUT_HI { get; set; }
        public string REP_TIMEOUT_HIHI { get; set; }
        public string REP_TIMEOUT_LO { get; set; }
        public string REP_TIMEOUT_LOLO { get; set; }
        public string RESET_ALLOWED_HI { get; set; }
        public string RESET_ALLOWED_HIHI { get; set; }
        public string RESET_ALLOWED_LO { get; set; }
        public string RESET_ALLOWED_LOLO { get; set; }
        public string RESET_COND { get; set; }
        public string RESET_PT { get; set; }
        public string RESET_TIMEOUT_HI { get; set; }
        public string RESET_TIMEOUT_HIHI { get; set; }
        public string RESET_TIMEOUT_LO { get; set; }
        public string RESET_TIMEOUT_LOLO { get; set; }
        public string RESOURCE_ID { get; set; }
        public string REV_CONV_EQ { get; set; }
        public string ROLLOVER_VAL { get; set; }
        public string SAFETY_PT { get; set; }
        public int SAMPLE_INTV { get; set; }
        public string SAMPLE_INTV_UNIT { get; set; }
        public string SCAN_RATE { get; set; }
        public string SETPOINT_HIGH { get; set; }
        public string SETPOINT_LOW { get; set; }
        public string TIME_OF_DAY { get; set; }
        public string TRIG_CK_PT { get; set; }
        public string TRIG_PT { get; set; }
        public string TRIG_REL { get; set; }
        public string TRIG_VAL { get; set; }
        public string UAFSET { get; set; }
        public string UPDATE_CRITERIA { get; set; }
        public string VARIANCE_VAL { get; set; }
        public int VARS { get; set; }

        public static CimplicityPointStructure Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<CimplicityPointStructure>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }

        public CimplicityPointStructure Clone()
        {
            return (CimplicityPointStructure)this.MemberwiseClone();
        }
    }
}
