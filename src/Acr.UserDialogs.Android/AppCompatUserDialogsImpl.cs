using System;
using System.Linq;
using Android.App;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Splat;
using AlertDialog = Android.Support.V7.App.AlertDialog;


namespace Acr.UserDialogs {

    public class AppCompatUserDialogsImpl : UserDialogsImpl {

        public AppCompatUserDialogsImpl(Func<Activity> getTopActivity) : base(getTopActivity) {}


        public override void Alert(AlertConfig config) {
            //var context = this.GetTopActivity();
            //var layout = new LinearLayout(context) {
            //    Orientation = Orientation.Vertical,
            //    OverScrollMode = OverScrollMode.IfContentScrolls
            //};
            //var txt = new TextView(context);

            Utils.RequestMainThread(() =>
                new AlertDialog
                    .Builder(this.GetTopActivity())
                    .SetCancelable(false)
                    .SetMessage(config.Message)
                    .SetTitle(config.Title)
					.SetPositiveButton(config.OkText, (o, e) => config.OnOk?.Invoke())
                    .ShowExt()
            );
        }


        public override void ActionSheet(ActionSheetConfig config) {
            var array = config
                .Options
                .Select(x => x.Text)
                .ToArray();

			var dlg = new AlertDialog
                .Builder(this.GetTopActivity())
				.SetCancelable(false)
				.SetTitle(config.Title);

            dlg.SetItems(array, (s, args) => config.Options[args.Which].Action?.Invoke());

			if (config.Destructive != null)
				dlg.SetNegativeButton(config.Destructive.Text, (s, a) => config.Destructive.Action?.Invoke());

			if (config.Cancel != null)
				dlg.SetNeutralButton(config.Cancel.Text, (s, a) => config.Cancel.Action?.Invoke());

			Utils.RequestMainThread(() => dlg.ShowExt());
        }


        public override void Confirm(ConfirmConfig config) {
            Utils.RequestMainThread(() =>
                new AlertDialog
                    .Builder(this.GetTopActivity())
                    .SetCancelable(false)
                    .SetMessage(config.Message)
                    .SetTitle(config.Title)
                    .SetPositiveButton(config.OkText, (s, a) => config.OnConfirm(true))
                    .SetNegativeButton(config.CancelText, (s, a) => config.OnConfirm(false))
                    .ShowExt()
            );
        }


        public override void Login(LoginConfig config) {
            var context = this.GetTopActivity();
            var txtUser = new EditText(context) {
                Hint = config.LoginPlaceholder,
                InputType = InputTypes.TextVariationVisiblePassword,
                Text = config.LoginValue ?? String.Empty
            };
            var txtPass = new EditText(context) {
                Hint = config.PasswordPlaceholder ?? "*"
            };
            this.SetInputType(txtPass, InputType.Password);

            var layout = new LinearLayout(context) {
                Orientation = Orientation.Vertical
            };

            txtUser.SetMaxLines(1);
            txtPass.SetMaxLines(1);

            layout.AddView(txtUser, ViewGroup.LayoutParams.MatchParent);
            layout.AddView(txtPass, ViewGroup.LayoutParams.MatchParent);

            Utils.RequestMainThread(() =>
                new AlertDialog
                    .Builder(context)
                    .SetCancelable(false)
                    .SetTitle(config.Title)
                    .SetMessage(config.Message)
                    .SetView(layout)
                    .SetPositiveButton(config.OkText, (s, a) =>
                        config.OnResult(new LoginResult(txtUser.Text, txtPass.Text, true))
                    )
                    .SetNegativeButton(config.CancelText, (s, a) =>
                        config.OnResult(new LoginResult(txtUser.Text, txtPass.Text, false))
                    )
                    .ShowExt()
            );
        }


        public override void Prompt(PromptConfig config) {
            Utils.RequestMainThread(() => {
                var activity = this.GetTopActivity();

                var txt = new EditText(activity) {
                    Hint = config.Placeholder
                };
				if (config.Text != null)
					txt.Text = config.Text;

                this.SetInputType(txt, config.InputType);

                var builder = new AlertDialog
                    .Builder(activity)
                    .SetCancelable(false)
                    .SetMessage(config.Message)
                    .SetTitle(config.Title)
                    .SetView(txt)
                    .SetPositiveButton(config.OkText, (s, a) =>
                        config.OnResult(new PromptResult {
                            Ok = true,
                            Text = txt.Text
                        })
					);

				if (config.IsCancellable) {
					builder.SetNegativeButton(config.CancelText, (s, a) =>
                        config.OnResult(new PromptResult {
                            Ok = false,
                            Text = txt.Text
                        })
					);
				}

				builder.Show();
            });
        }


        public override void Toast(ToastConfig cfg) {
            var top = this.GetTopActivity();
            var view = top.Window.DecorView.RootView;
            var snackBar = Snackbar.Make(view, cfg.Text, (int)cfg.Duration.TotalMilliseconds);
            snackBar.View.Background = new ColorDrawable(cfg.BackgroundColor.ToNative());

            //android.support.design.R.id.snackbar_text // TODO
            //snackBar.View.FindViewById<TextView>().SetTextColor

            snackBar.View.Click += (sender, args) => {
                snackBar.Dismiss();
                cfg.Action?.Invoke();
            };
            ////if (cfg.BackgroundColor != null)
            ////    snackBar.SetActionTextColor()
            //if (cfg.OnTap != null)
            //    snackBar.SetAction("Ok", x => cfg.OnTap?.Invoke());

            Utils.RequestMainThread(snackBar.Show);
        }
    }
}
/*
Snackbar snack = Snackbar.make(...);
ViewGroup group = (ViewGroup) snack.getView();
for (int i = 0; i < group.getChildCount(); i++) {
    View v = group.getChildAt(i);
    if (v instanceof TextView) {
        TextView t = (TextView) v;
        t.setTextColor(...)
    }
}
snack.show();
*/