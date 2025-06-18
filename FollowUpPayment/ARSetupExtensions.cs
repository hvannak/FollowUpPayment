using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects;
using System.Collections.Generic;
using System.Globalization;
using System;

namespace PX.Objects.AR
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class ARSetupExt : PXCacheExtension<PX.Objects.AR.ARSetup>
  {
    #region UsrTelegramCC
    [PXDBString(50)]
    [PXUIField(DisplayName="Telegram CC")]

    public virtual string UsrTelegramCC { get; set; }
    public abstract class usrTelegramCC : PX.Data.BQL.BqlString.Field<usrTelegramCC> { }
        #endregion
        #region UsrSmsuser
        [PXDBString(50)]
        [PXUIField(DisplayName = "Sms User")]

        public virtual string UsrSmsuser { get; set; }
        public abstract class usrSmsuser : PX.Data.BQL.BqlString.Field<usrSmsuser> { }
        #endregion

        #region UsrSmspass
        [PXDBString(200)]
        [PXUIField(DisplayName = "Sms Password")]

        public virtual string UsrSmspass { get; set; }
        public abstract class usrSmspass : PX.Data.BQL.BqlString.Field<usrSmspass> { }
        #endregion

        #region UsrSmsurl
        [PXDBString(500)]
        [PXUIField(DisplayName = "Sms Url")]

        public virtual string UsrSmsurl { get; set; }
        public abstract class usrSmsurl : PX.Data.BQL.BqlString.Field<usrSmsurl> { }
        #endregion
        #region UsrSmssender
        [PXDBString(50)]
        [PXUIField(DisplayName = "Sms Sender")]

        public virtual string UsrSmssender { get; set; }
        public abstract class usrSmssender : PX.Data.BQL.BqlString.Field<usrSmssender> { }
        #endregion
    }
}