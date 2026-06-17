import AuthPage from "@/app/components/AuthPage";

export default async function LoginPage({
  searchParams,
}: {
  searchParams: Promise<{ redirect?: string }>;
}) {
  const resolved = await searchParams;
  return <AuthPage mode="login" redirect={resolved.redirect || "/dashboard"} />;
}