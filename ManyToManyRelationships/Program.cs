using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

public class Maquina
{
    public Guid? Codigo { get; set; }
    public ICollection<Grupo>? Grupos { get; set; }
    public Maquina() => Codigo = Guid.NewGuid();
}
public class Grupo
{
    public Guid? Codigo { get; set; }
    public ICollection<Maquina>? Maquinas { get; set; }
    public Grupo() => Codigo = Guid.NewGuid();
}
public class MaquinaGrupo
{
    public Guid? MaquinaCodigo { get; set; }
    public Guid? GrupoCodigo { get; set; }
}
public class DataContext : DbContext
{
    public DbSet<Maquina> Maquina { get; set; }
    public DbSet<Grupo> Grupo { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var maquinaEntity = modelBuilder.Entity<Maquina>();
        maquinaEntity.ToTable("maquina", _ => _.IsTemporal(
            ttb =>
            {
                ttb.HasPeriodStart("data_inicio_vigencia");
                ttb.HasPeriodEnd("data_fim_vigencia");
                ttb.UseHistoryTable("th_maquina");
            }
        ));
        maquinaEntity.HasKey(x => x.Codigo);

        var grupoEntity = modelBuilder.Entity<Grupo>();
        grupoEntity.ToTable("grupo", _ => _.IsTemporal(
            ttb =>
            {
                ttb.HasPeriodStart("data_inicio_vigencia");
                ttb.HasPeriodEnd("data_fim_vigencia");
                ttb.UseHistoryTable("th_grupo");
            }
        ));
        grupoEntity.HasKey(x => x.Codigo);
        grupoEntity
           .HasMany(p => p.Maquinas)
           .WithMany(p => p.Grupos)
           .UsingEntity<MaquinaGrupo>(
               "maquina_grupo",
               j => j
                   .HasOne<Maquina>()
                   .WithMany()
                   .HasForeignKey(_ => _.MaquinaCodigo),
               j => j
                   .HasOne<Grupo>()
                   .WithMany()
                   .HasForeignKey(_ => _.GrupoCodigo),
               c => c.ToTable("maquina_grupo", _ => _.IsTemporal(
                    ttb =>
                    {
                        ttb.HasPeriodStart("data_inicio_vigencia");
                        ttb.HasPeriodEnd("data_fim_vigencia");
                        ttb.UseHistoryTable("th_equipamento_grupo");
                    })));
    }
}
public class GrupoRepositorio
{
    DataContext _dataContext;
    public GrupoRepositorio(DataContext dataContext) => _dataContext = dataContext;
    public async Task Inserir(Grupo grupo)
    {
        _dataContext.Maquina.AttachRange(grupo.Maquinas);
        await _dataContext.Grupo.AddAsync(grupo);

        await _dataContext.SaveChangesAsync();
    }
    public async Task Alterar(Grupo grupo)
    {
        _dataContext.Maquina.AttachRange(grupo.Maquinas);
        _dataContext.Grupo.Attach(grupo);
        _dataContext.Entry(grupo).State = EntityState.Modified;
        await _dataContext.SaveChangesAsync();
    }
}



// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
